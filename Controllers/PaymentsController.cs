using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using VAM.DTOs;
using VAM.Services;

namespace VAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentsController(IPaymentService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize, search);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePaymentDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(UpdatePaymentDto dto)
        {
            await _service.UpdateAsync(dto);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Creates a PayOS checkout link for an order. Buyer is redirected to VietQR payment.
        /// </summary>
        [Authorize]
        [HttpPost("checkout/{orderId}")]
        public async Task<IActionResult> Checkout(int orderId)
        {
            var checkoutUrl = await _service.CreateCheckoutUrlAsync(orderId);
            return Ok(new { checkoutUrl });
        }

        /// <summary>
        /// PayOS webhook endpoint. Called by PayOS when payment status changes.
        /// Must be anonymous for PayOS to call it.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] Webhook webhookBody)
        {
            await _service.ProcessWebhookPayloadAsync(webhookBody);
            return Ok();
        }
    }
}