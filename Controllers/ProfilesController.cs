using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using VAM.DTOs;
using VAM.Services;

namespace VAM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            throw new System.Exception("Invalid user claims");
        }

        [HttpPost("seller")]
        public async Task<IActionResult> CreateSellerProfile([FromBody] CreateSellerProfileDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _profileService.CreateSellerProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("business")]
        public async Task<IActionResult> CreateBusinessProfile([FromBody] CreateBusinessProfileDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _profileService.CreateBusinessProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("seller/me")]
        public async Task<IActionResult> GetMySellerProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _profileService.GetMySellerProfileAsync(userId);
                if (result == null) return NotFound(new { message = "Seller profile not found" });
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("business/me")]
        public async Task<IActionResult> GetMyBusinessProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _profileService.GetMyBusinessProfileAsync(userId);
                if (result == null) return NotFound(new { message = "Business profile not found" });
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
