using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using VAM.DTOs;
using VAM.Services;

namespace VAM.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public AdminProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private int GetAdminId()
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(adminIdStr, out int adminId))
            {
                return adminId;
            }
            throw new System.Exception("Invalid admin claims");
        }

        [HttpGet("seller/pending")]
        public async Task<IActionResult> GetPendingSellerProfiles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _profileService.GetPendingSellerProfilesAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("business/pending")]
        public async Task<IActionResult> GetPendingBusinessProfiles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _profileService.GetPendingBusinessProfilesAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("seller/{id}/approve")]
        public async Task<IActionResult> ApproveSellerProfile(int id, [FromBody] ApproveProfileDto dto)
        {
            try
            {
                var adminId = GetAdminId();
                await _profileService.ApproveSellerProfileAsync(id, adminId, dto);
                return Ok(new { message = "Seller profile approval status updated successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("business/{id}/approve")]
        public async Task<IActionResult> ApproveBusinessProfile(int id, [FromBody] ApproveProfileDto dto)
        {
            try
            {
                var adminId = GetAdminId();
                await _profileService.ApproveBusinessProfileAsync(id, adminId, dto);
                return Ok(new { message = "Business profile approval status updated successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
