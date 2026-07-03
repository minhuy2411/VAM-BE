using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VAM.Data;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;

namespace VAM.Services
{
    public class ProductService : ServiceBase<Product, ProductDto, CreateProductDto, UpdateProductDto>, IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFirebaseStorageService _firebaseStorage;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            IFirebaseStorageService firebaseStorage) 
            : base(unitOfWork, unitOfWork.Products, mapper)
        {
            _context = context;
            _firebaseStorage = firebaseStorage;
        }

        public new async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .Include(p => p.Category)
                .Include(p => p.Farm)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return null;

            var dto = _mapper.Map<ProductDto>(product);
            dto.SellerName = product.Seller?.Name;
            dto.CategoryName = product.Category?.Name;
            dto.FarmName = product.Farm?.FarmName;

            if (!string.IsNullOrWhiteSpace(product.ImageUrls))
            {
                try
                {
                    dto.ImageUrls = JsonSerializer.Deserialize<List<string>>(product.ImageUrls) ?? new List<string>();
                }
                catch
                {
                    dto.ImageUrls = new List<string> { product.ImageUrls };
                }
            }

            // Fetch reviews for rating
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == id)
                .ToListAsync();

            if (reviews.Any())
            {
                dto.AverageRating = Math.Round(reviews.Average(r => r.Rating), 1);
                dto.TotalReviews = reviews.Count;
            }

            return dto;
        }

        public async Task<PaginatedResult<ProductDto>> GetFilteredAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Seller)
                .Include(p => p.Category)
                .Include(p => p.Farm)
                .AsQueryable();

            // Filter by Status (default to "approved" if not specified)
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(p => p.Status.ToLower() == filter.Status.ToLower());
            }
            else
            {
                query = query.Where(p => p.Status.ToLower() == "approved");
            }

            // Search by Name or Description
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchLower) || 
                                         (p.Description != null && p.Description.ToLower().Contains(searchLower)));
            }

            // Price filtering
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // Category filtering
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            // Farm filtering
            if (filter.FarmId.HasValue)
            {
                query = query.Where(p => p.FarmId == filter.FarmId.Value);
            }

            // Location filtering (via Farm)
            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                var locationLower = filter.Location.ToLower();
                query = query.Where(p => p.Farm != null && p.Farm.Location.ToLower().Contains(locationLower));
            }

            var productsList = await query.ToListAsync();

            // Get product IDs and fetch ratings
            var productIds = productsList.Select(p => p.Id).ToList();
            var reviews = await _context.Reviews
                .Where(r => productIds.Contains(r.ProductId))
                .ToListAsync();

            var reviewStats = reviews
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => new
                {
                    AvgRating = g.Average(r => r.Rating),
                    Count = g.Count()
                });

            // Filter by MinRating if requested
            if (filter.MinRating.HasValue)
            {
                productsList = productsList.Where(p => 
                    reviewStats.ContainsKey(p.Id) && reviewStats[p.Id].AvgRating >= filter.MinRating.Value
                ).ToList();
            }

            int totalCount = productsList.Count;
            var pagedProducts = productsList
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = pagedProducts.Select(p =>
            {
                var dto = _mapper.Map<ProductDto>(p);
                dto.SellerName = p.Seller?.Name;
                dto.CategoryName = p.Category?.Name;
                dto.FarmName = p.Farm?.FarmName;
                
                if (!string.IsNullOrWhiteSpace(p.ImageUrls))
                {
                    try
                    {
                        dto.ImageUrls = JsonSerializer.Deserialize<List<string>>(p.ImageUrls) ?? new List<string>();
                    }
                    catch
                    {
                        dto.ImageUrls = new List<string> { p.ImageUrls };
                    }
                }

                if (reviewStats.TryGetValue(p.Id, out var stats))
                {
                    dto.AverageRating = Math.Round(stats.AvgRating, 1);
                    dto.TotalReviews = stats.Count;
                }

                return dto;
            }).ToList();

            return new PaginatedResult<ProductDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ProductDto> CreateProductWithImagesAsync(CreateProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            
            // Auto update status to out_of_stock if quantity <= 0
            if (product.Quantity <= 0)
            {
                product.Status = "out_of_stock";
            }
            else
            {
                product.Status = "pending"; // Default status upon creation
            }

            if (dto.Images != null && dto.Images.Count > 0)
            {
                var uploadedUrls = new List<string>();
                foreach (var file in dto.Images)
                {
                    var url = await _firebaseStorage.UploadFileAsync(file, "products");
                    uploadedUrls.Add(url);
                }
                product.ImageUrls = JsonSerializer.Serialize(uploadedUrls);
            }

            await _repository.CreateAsync(product);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<ProductDto>(product);
            if (!string.IsNullOrWhiteSpace(product.ImageUrls))
            {
                try { resultDto.ImageUrls = JsonSerializer.Deserialize<List<string>>(product.ImageUrls) ?? new List<string>(); }
                catch { resultDto.ImageUrls = new List<string> { product.ImageUrls }; }
            }

            return resultDto;
        }

        public async Task UpdateProductWithImagesAsync(UpdateProductDto dto)
        {
            var product = await _repository.GetByIdAsync(dto.Id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {dto.Id} not found.");
            }

            if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
            if (dto.FarmId.HasValue) product.FarmId = dto.FarmId.Value;
            if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name;
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (dto.Quantity.HasValue) product.Quantity = dto.Quantity.Value;
            if (!string.IsNullOrWhiteSpace(dto.Unit)) product.Unit = dto.Unit;
            if (!string.IsNullOrWhiteSpace(dto.Status)) product.Status = dto.Status;

            // Auto update out_of_stock
            if (product.Quantity <= 0)
            {
                product.Status = "out_of_stock";
            }

            var finalUrls = dto.ExistingImageUrls ?? new List<string>();
            if (dto.NewImages != null && dto.NewImages.Count > 0)
            {
                foreach (var file in dto.NewImages)
                {
                    var url = await _firebaseStorage.UploadFileAsync(file, "products");
                    finalUrls.Add(url);
                }
            }

            product.ImageUrls = JsonSerializer.Serialize(finalUrls);

            _repository.Update(product);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ApproveProductAsync(int id, ApproveProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            product.Status = dto.Status.ToLower();
            _repository.Update(product);
            await _unitOfWork.CompleteAsync();
        }
    }
}