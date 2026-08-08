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
                .AsNoTracking()
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

            // Rating stats (cached on Product entity, fallback to Reviews query if 0)
            if (product.AverageRating > 0 || product.TotalReviews > 0)
            {
                dto.AverageRating = product.AverageRating;
                dto.TotalReviews = product.TotalReviews;
            }
            else
            {
                var reviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(r => r.ProductId == id)
                    .ToListAsync();

                if (reviews.Any())
                {
                    dto.AverageRating = Math.Round(reviews.Average(r => r.Rating), 1);
                    dto.TotalReviews = reviews.Count;
                }
            }

            // Fetch seller profile if present
            var sellerProfile = await _context.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.UserId == product.SellerId && !sp.IsDeleted);

            if (sellerProfile != null)
            {
                dto.SellerProfileId = sellerProfile.Id;
                dto.SupplierName = !string.IsNullOrWhiteSpace(sellerProfile.FarmName) ? sellerProfile.FarmName : (product.Farm?.FarmName ?? product.Seller?.Name);
                dto.SupplierLocation = !string.IsNullOrWhiteSpace(sellerProfile.FarmAddress) ? sellerProfile.FarmAddress : product.Farm?.Location;
                dto.IsSupplierVerified = sellerProfile.Status == Entities.ProfileStatus.APPROVED;
                dto.SupplierRating = sellerProfile.AverageRating > 0 ? sellerProfile.AverageRating : 5.0;
            }
            else
            {
                dto.SellerProfileId = product.FarmId ?? product.SellerId;
                dto.SupplierName = product.Farm?.FarmName ?? product.Seller?.Name ?? "Hộ nuôi Hải Sản Việt Nam";
                dto.SupplierLocation = product.Farm?.Location ?? "Việt Nam";
                dto.IsSupplierVerified = true;
                dto.SupplierRating = 5.0;
            }

            return dto;
        }

        public async Task<PaginatedResult<ProductDto>> GetFilteredAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Seller)
                .Include(p => p.Category)
                .Include(p => p.Farm)
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            // Filter by Status (If status is provided and not "all", filter by status; if empty/null/"all", fetch all statuses)
            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status.ToLower() != "all")
            {
                query = query.Where(p => p.Status.ToLower() == filter.Status.ToLower());
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

            // Seller filtering
            if (filter.SellerId.HasValue)
            {
                query = query.Where(p => p.SellerId == filter.SellerId.Value);
            }

            // Location filtering (via Farm)
            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                var locationLower = filter.Location.ToLower();
                query = query.Where(p => p.Farm != null && p.Farm.Location.ToLower().Contains(locationLower));
            }

            // Wholesale filtering
            if (filter.IsWholesale.HasValue)
            {
                query = query.Where(p => p.IsWholesale == filter.IsWholesale.Value);
            }

            // Filter by MinRating at Database level
            if (filter.MinRating.HasValue)
            {
                query = query.Where(p => p.AverageRating >= filter.MinRating.Value);
            }

            // Total count evaluated at Database level
            int totalCount = await query.CountAsync();

            // Pagination evaluated at Database level (SQL OFFSET & LIMIT)
            var pagedProducts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = pagedProducts.Select(p =>
            {
                var dto = _mapper.Map<ProductDto>(p);
                dto.SellerName = p.Seller?.Name;
                dto.CategoryName = p.Category?.Name;
                dto.FarmName = p.Farm?.FarmName;
                dto.AverageRating = p.AverageRating;
                dto.TotalReviews = p.TotalReviews;
                
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
            if (dto.MinOrderQuantity.HasValue) product.MinOrderQuantity = dto.MinOrderQuantity.Value;
            if (dto.IsWholesale.HasValue) product.IsWholesale = dto.IsWholesale.Value;
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