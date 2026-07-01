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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
        {
            filter ??= new ProductFilterDto();
            var result = await _service.GetFilteredAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            if (!User.IsInRole("admin"))
            {
                dto.SellerId = GetUserId();
            }
            var result = await _service.CreateProductWithImagesAsync(dto);
            return Ok(result);
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
        {
            dto.Id = id;
            if (!User.IsInRole("admin"))
            {
                var existingProduct = await _service.GetByIdAsync(id);
                if (existingProduct == null) return NotFound();
                if (existingProduct.SellerId != GetUserId())
                {
                    return Forbid();
                }
            }

            await _service.UpdateProductWithImagesAsync(dto);
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveProductDto dto)
        {
            await _service.ApproveProductAsync(id, dto);
            return NoContent();
        }

        [Authorize(Roles = "seller,admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("admin"))
            {
                var existingProduct = await _service.GetByIdAsync(id);
                if (existingProduct == null) return NotFound();
                if (existingProduct.SellerId != GetUserId())
                {
                    return Forbid();
                }
            }

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}