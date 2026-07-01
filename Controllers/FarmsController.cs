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
    public class FarmsController : ControllerBase
    {
        private readonly IFarmService _service;

        public FarmsController(IFarmService service)
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
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize, search);
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
        public async Task<IActionResult> Create(CreateFarmDto dto)
        {
            if (!User.IsInRole("admin"))
            {
                dto.SellerId = GetUserId();
            }
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPut]
        public async Task<IActionResult> Update(UpdateFarmDto dto)
        {
            await _service.UpdateAsync(dto);
            return NoContent();
        }

        [Authorize(Roles = "seller,admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}