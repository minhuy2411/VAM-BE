using System.Threading.Tasks;
using VAM.DTOs;

namespace VAM.Services
{
    public interface IProfileService
    {
        // For users
        Task<SellerProfileDto> CreateSellerProfileAsync(int userId, CreateSellerProfileDto dto);
        Task<BusinessProfileDto> CreateBusinessProfileAsync(int userId, CreateBusinessProfileDto dto);
        Task<SellerProfileDto?> GetMySellerProfileAsync(int userId);
        Task<BusinessProfileDto?> GetMyBusinessProfileAsync(int userId);
        Task<PaginatedResult<SellerProfileDto>> GetApprovedSellerProfilesAsync(int page, int pageSize, string? search = null);
        Task<SellerDetailDto?> GetSellerDetailByIdAsync(int id);

        // For admin
        Task<PaginatedResult<SellerProfileDto>> GetPendingSellerProfilesAsync(int page, int pageSize);
        Task<PaginatedResult<BusinessProfileDto>> GetPendingBusinessProfilesAsync(int page, int pageSize);
        Task ApproveSellerProfileAsync(int profileId, int adminId, ApproveProfileDto dto);
        Task ApproveBusinessProfileAsync(int profileId, int adminId, ApproveProfileDto dto);
    }
}
