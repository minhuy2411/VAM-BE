using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using VAM.DTOs;
using VAM.Services;

namespace VAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
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
            throw new System.Exception("Invalid user claims");
        }

        private string GetUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }

        /// <summary>
        /// Get all orders (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Get current buyer's orders
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetOrdersByBuyerAsync(GetUserId(), pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get orders containing seller's products
        /// </summary>
        [HttpGet("seller-orders")]
        [Authorize(Roles = "seller,admin")]
        public async Task<IActionResult> GetSellerOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetOrdersBySellerAsync(GetUserId(), pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Create a new order with automatic price calculation and stock validation.
        /// BuyerId is set from JWT token.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            dto.BuyerId = GetUserId();
            var result = await _service.CreateOrderAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update order status (state machine).
        /// Valid transitions: pending→confirmed, pending→cancelled, confirmed→shipping,
        /// confirmed→cancelled, shipping→completed
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            await _service.UpdateOrderStatusAsync(id, userId, role, dto);
            return NoContent();
        }

        /// <summary>
        /// Delete order (admin only, soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}