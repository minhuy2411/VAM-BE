using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VAM.DTOs;
using VAM.Services;

namespace VAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewsController(IReviewService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            throw new Exception("Thông tin xác thực không hợp lệ.");
        }

        [HttpGet]
        public async Task<IActionResult> GetFiltered([FromQuery] ReviewFilterDto filter)
        {
            var result = await _service.GetFilteredReviewsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("product/{productId}/summary")]
        public async Task<IActionResult> GetProductRatingSummary(int productId)
        {
            var summary = await _service.GetProductRatingSummaryAsync(productId);
            return Ok(summary);
        }

        [HttpGet("seller/{sellerId}/summary")]
        public async Task<IActionResult> GetSellerRatingSummary(int sellerId)
        {
            var summary = await _service.GetSellerRatingSummaryAsync(sellerId);
            return Ok(summary);
        }

        [Authorize]
        [HttpGet("can-review")]
        public async Task<IActionResult> CanUserReview([FromQuery] int orderId, [FromQuery] int productId)
        {
            var canReview = await _service.CanUserReviewProductAsync(GetUserId(), orderId, productId);
            return Ok(new { canReview });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateReviewDto dto)
        {
            try
            {
                var buyerId = GetUserId();
                var result = await _service.CreateReviewWithImagesAsync(buyerId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateReviewDto dto)
        {
            try
            {
                dto.Id = id;
                var buyerId = GetUserId();
                await _service.UpdateReviewWithImagesAsync(buyerId, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPost("{id}/reply")]
        public async Task<IActionResult> SellerReply(int id, [FromBody] SellerReplyDto dto)
        {
            try
            {
                dto.ReviewId = id;
                var sellerId = GetUserId();
                await _service.SellerReplyAsync(sellerId, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var review = await _service.GetByIdAsync(id);
                if (review == null) return NotFound();

                await _service.DeleteAsync(id);
                await _service.RecalculateProductAndSellerRatingAsync(review.ProductId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}