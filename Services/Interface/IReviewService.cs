using System.Threading.Tasks;
using VAM.DTOs;

namespace VAM.Services
{
    public interface IReviewService : IServiceBase<ReviewDto, CreateReviewDto, UpdateReviewDto>
    {
        Task<ReviewDto> CreateReviewWithImagesAsync(int buyerId, CreateReviewDto dto);
        Task UpdateReviewWithImagesAsync(int buyerId, UpdateReviewDto dto);
        Task SellerReplyAsync(int sellerId, SellerReplyDto dto);
        Task<PaginatedResult<ReviewDto>> GetFilteredReviewsAsync(ReviewFilterDto filter);
        Task<ProductRatingSummaryDto> GetProductRatingSummaryAsync(int productId);
        Task<SellerRatingSummaryDto> GetSellerRatingSummaryAsync(int sellerId);
        Task<bool> CanUserReviewProductAsync(int userId, int orderId, int productId);
        Task RecalculateProductAndSellerRatingAsync(int productId);
    }
}