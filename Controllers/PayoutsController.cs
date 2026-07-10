using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAM.Data;
using VAM.DTOs;

namespace VAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayoutsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PayoutsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all payout transactions (admin). Supports pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.PayoutTransactions
                .Include(pt => pt.Order)
                .Include(pt => pt.SellerProfile)
                .OrderByDescending(pt => pt.CreatedAt)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pt => new PayoutTransactionDto
                {
                    Id = pt.Id,
                    OrderId = pt.OrderId,
                    SellerProfileId = pt.SellerProfileId,
                    Amount = pt.Amount,
                    PlatformFee = pt.PlatformFee,
                    BankName = pt.BankName,
                    AccountNumber = pt.AccountNumber,
                    AccountHolderName = pt.AccountHolderName,
                    Status = pt.Status,
                    Note = pt.Note,
                    TransactionId = pt.TransactionId,
                    CreatedAt = pt.CreatedAt
                })
                .ToListAsync();

            return Ok(new PaginatedResult<PayoutTransactionDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// Admin manually confirms a failed payout (e.g., after manual bank transfer).
        /// </summary>
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmManual(int id)
        {
            var payout = await _context.PayoutTransactions
                .FirstOrDefaultAsync(pt => pt.Id == id && !pt.IsDeleted);

            if (payout == null)
                return NotFound(new { message = "Payout transaction not found" });

            if (payout.Status == "completed")
                return BadRequest(new { message = "Payout already completed" });

            payout.Status = "completed";
            payout.Note = (payout.Note ?? "") + " | Xác nhận chuyển tay bởi Admin vào " + DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            payout.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Payout confirmed successfully", payoutId = id });
        }
    }
}
