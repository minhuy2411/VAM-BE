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
using VAM.Exceptions;
using VAM.Repositories;

namespace VAM.Services
{
    public class ReviewService : ServiceBase<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>, IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFirebaseStorageService _storageService;

        public ReviewService(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            ApplicationDbContext context, 
            IFirebaseStorageService storageService) 
            : base(unitOfWork, unitOfWork.Reviews, mapper)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<ReviewDto> CreateReviewWithImagesAsync(int buyerId, CreateReviewDto dto)
        {
            // 1. Verify Order exists and belongs to buyer
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.BuyerId == buyerId);

            if (order == null)
            {
                throw new AppException("Đơn hàng không tồn tại hoặc không thuộc quyền sở hữu của bạn.");
            }

            // 2. Check Order Status
            if (string.IsNullOrEmpty(order.Status) || order.Status.ToLower() != "completed")
            {
                throw new AppException("Chỉ có thể đánh giá sản phẩm từ đơn hàng đã hoàn thành.");
            }

            // 3. Check Order contains product
            var orderItemExists = order.OrderItems.Any(oi => oi.ProductId == dto.ProductId);
            if (!orderItemExists)
            {
                throw new AppException("Sản phẩm không nằm trong đơn hàng này.");
            }

            // 4. Prevent Duplicate Review
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId && r.ProductId == dto.ProductId && r.UserId == buyerId);

            if (existingReview != null)
            {
                throw new AppException("Bạn đã gửi đánh giá cho sản phẩm này trong đơn hàng này rồi.");
            }

            // 5. Upload images if provided
            var imageUrlsList = new List<string>();
            if (dto.Images != null && dto.Images.Count > 0)
            {
                foreach (var image in dto.Images)
                {
                    if (image.Length > 0)
                    {
                        var url = await _storageService.UploadFileAsync(image, "reviews");
                        imageUrlsList.Add(url);
                    }
                }
            }

            var jsonImageUrls = imageUrlsList.Count > 0 
                ? JsonSerializer.Serialize(imageUrlsList) 
                : null;

            // 6. Create Review Entity
            var review = new Review
            {
                OrderId = dto.OrderId,
                UserId = buyerId,
                ProductId = dto.ProductId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                ImageUrls = jsonImageUrls,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // 7. Recalculate Ratings for Product & Seller Profile
            await RecalculateProductAndSellerRatingAsync(dto.ProductId);

            // Fetch created review with User & Product navigation properties
            var createdReview = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            return _mapper.Map<ReviewDto>(createdReview);
        }

        public async Task UpdateReviewWithImagesAsync(int buyerId, UpdateReviewDto dto)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == dto.Id && r.UserId == buyerId);
            if (review == null)
            {
                throw new AppException("Không tìm thấy đánh giá hoặc bạn không có quyền chỉnh sửa.");
            }

            if (dto.Rating.HasValue)
            {
                review.Rating = dto.Rating.Value;
            }

            if (dto.Comment != null)
            {
                review.Comment = dto.Comment;
            }

            // Process image updates
            var finalUrls = new List<string>();
            if (dto.KeepImageUrls != null)
            {
                finalUrls.AddRange(dto.KeepImageUrls);
            }

            if (dto.NewImages != null && dto.NewImages.Count > 0)
            {
                foreach (var image in dto.NewImages)
                {
                    if (image.Length > 0)
                    {
                        var url = await _storageService.UploadFileAsync(image, "reviews");
                        finalUrls.Add(url);
                    }
                }
            }

            review.ImageUrls = finalUrls.Count > 0 ? JsonSerializer.Serialize(finalUrls) : null;
            review.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            // Recalculate Rating Stats
            await RecalculateProductAndSellerRatingAsync(review.ProductId);
        }

        public async Task SellerReplyAsync(int sellerId, SellerReplyDto dto)
        {
            var review = await _context.Reviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == dto.ReviewId);

            if (review == null || review.Product == null)
            {
                throw new AppException("Đánh giá không tồn tại.");
            }

            if (review.Product.SellerId != sellerId)
            {
                throw new AppException("Bạn không có quyền phản hồi đánh giá sản phẩm của gian hàng khác.");
            }

            review.SellerReply = dto.Reply;
            review.SellerRepliedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResult<ReviewDto>> GetFilteredReviewsAsync(ReviewFilterDto filter)
        {
            filter ??= new ReviewFilterDto();

            var query = _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .AsQueryable();

            if (filter.ProductId.HasValue)
            {
                query = query.Where(r => r.ProductId == filter.ProductId.Value);
            }

            if (filter.SellerId.HasValue)
            {
                query = query.Where(r => r.Product != null && r.Product.SellerId == filter.SellerId.Value);
            }

            if (filter.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == filter.Rating.Value);
            }

            if (filter.HasImages.HasValue && filter.HasImages.Value)
            {
                query = query.Where(r => !string.IsNullOrEmpty(r.ImageUrls) && r.ImageUrls != "[]");
            }

            query = filter.SortBy?.ToLower() switch
            {
                "highest_rating" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                "lowest_rating" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var totalItems = await query.CountAsync();
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<ReviewDto>>(items);

            return new PaginatedResult<ReviewDto>
            {
                Items = dtos,
                TotalCount = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ProductRatingSummaryDto> GetProductRatingSummaryAsync(int productId)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            var total = reviews.Count;
            var avg = total > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0.0;

            return new ProductRatingSummaryDto
            {
                ProductId = productId,
                AverageRating = avg,
                TotalReviews = total,
                FiveStarCount = reviews.Count(r => r.Rating == 5),
                FourStarCount = reviews.Count(r => r.Rating == 4),
                ThreeStarCount = reviews.Count(r => r.Rating == 3),
                TwoStarCount = reviews.Count(r => r.Rating == 2),
                OneStarCount = reviews.Count(r => r.Rating == 1),
                WithImagesCount = reviews.Count(r => !string.IsNullOrEmpty(r.ImageUrls) && r.ImageUrls != "[]")
            };
        }

        public async Task<SellerRatingSummaryDto> GetSellerRatingSummaryAsync(int sellerId)
        {
            var sellerProfile = await _context.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == sellerId);

            var sellerProductIds = await _context.Products
                .AsNoTracking()
                .Where(p => p.SellerId == sellerId)
                .Select(p => p.Id)
                .ToListAsync();

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => sellerProductIds.Contains(r.ProductId))
                .ToListAsync();

            var total = reviews.Count;
            var avg = total > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0.0;
            var repliedCount = reviews.Count(r => !string.IsNullOrEmpty(r.SellerReply));
            var responseRate = total > 0 ? Math.Round((double)repliedCount / total * 100, 1) : 0.0;

            return new SellerRatingSummaryDto
            {
                SellerId = sellerId,
                FarmName = sellerProfile?.FarmName ?? string.Empty,
                AverageRating = avg,
                TotalReviews = total,
                FiveStarCount = reviews.Count(r => r.Rating == 5),
                FourStarCount = reviews.Count(r => r.Rating == 4),
                ThreeStarCount = reviews.Count(r => r.Rating == 3),
                TwoStarCount = reviews.Count(r => r.Rating == 2),
                OneStarCount = reviews.Count(r => r.Rating == 1),
                ResponseRate = responseRate
            };
        }

        public async Task<bool> CanUserReviewProductAsync(int userId, int orderId, int productId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == userId);

            if (order == null || string.IsNullOrEmpty(order.Status) || order.Status.ToLower() != "completed")
            {
                return false;
            }

            var containsProduct = order.OrderItems.Any(oi => oi.ProductId == productId);
            if (!containsProduct)
            {
                return false;
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.OrderId == orderId && r.ProductId == productId && r.UserId == userId);

            return !alreadyReviewed;
        }

        public async Task RecalculateProductAndSellerRatingAsync(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return;

            var productReviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            product.TotalReviews = productReviews.Count;
            product.AverageRating = productReviews.Any() ? Math.Round(productReviews.Average(r => r.Rating), 1) : 0.0;

            // Also recalculate for SellerProfile
            int sellerUserId = product.SellerId;
            var sellerProfile = await _context.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == sellerUserId);
            if (sellerProfile != null)
            {
                var sellerProductIds = await _context.Products
                    .Where(p => p.SellerId == sellerUserId)
                    .Select(p => p.Id)
                    .ToListAsync();

                var sellerReviews = await _context.Reviews
                    .Where(r => sellerProductIds.Contains(r.ProductId))
                    .ToListAsync();

                sellerProfile.TotalReviews = sellerReviews.Count;
                sellerProfile.AverageRating = sellerReviews.Any() ? Math.Round(sellerReviews.Average(r => r.Rating), 1) : 0.0;
            }

            await _context.SaveChangesAsync();
        }
    }
}