using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using VAM.Data;
using VAM.DTOs;
using VAM.Entities;
using VAM.Exceptions;
using VAM.Repositories;

namespace VAM.Services
{
    public class PaymentService : ServiceBase<Payment, PaymentDto, CreatePaymentDto, UpdatePaymentDto>, IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly PayOSClient _payOSClient;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            PayOSClient payOSClient,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
            : base(unitOfWork, unitOfWork.Payments, mapper)
        {
            _context = context;
            _payOSClient = payOSClient;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a PayOS payment link for 100% of the order total.
        /// Returns the checkout URL for the buyer.
        /// </summary>
        public async Task<string> CreateCheckoutUrlAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
                throw new AppException("Order not found", 404, "ORDER_NOT_FOUND");

            if (order.Status != "pending")
                throw new AppException("Order is not in a payable state", 400, "ORDER_NOT_PAYABLE");

            // Check if a payment already exists for this order
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == "completed" && !p.IsDeleted);
            if (existingPayment != null)
                throw new AppException("Order has already been paid", 400, "ORDER_ALREADY_PAID");

            // Build item list for PayOS
            var items = order.OrderItems.Select(oi => new PaymentLinkItem
            {
                Name = $"Order item #{oi.ProductId}",
                Quantity = (int)oi.Quantity,
                Price = (long)oi.Price
            }).ToList();

            // Generate a unique order code for PayOS (use timestamp + orderId to avoid conflicts)
            long orderCode = long.Parse($"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{orderId}");

            var returnUrl = _configuration["PayOS:ReturnUrl"] ?? "https://vam.example.com/payment/success";
            var cancelUrl = _configuration["PayOS:CancelUrl"] ?? "https://vam.example.com/payment/cancel";

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (long)order.TotalPrice,
                Description = $"DH #{orderId}",
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl,
                Items = items
            };

            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

            // Create a pending Payment record
            var payment = new Payment
            {
                OrderId = orderId,
                Method = "PayOS",
                Amount = order.TotalPrice,
                Status = "pending",
                PaymentDate = DateTimeOffset.UtcNow
            };
            await _unitOfWork.Payments.CreateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return paymentLink.CheckoutUrl;
        }

        /// <summary>
        /// Handles PayOS webhook:
        /// 1. Verifies webhook signature
        /// 2. Updates Payment.Status = "completed" and Order.Status = "confirmed"
        /// 3. Calculates 95% payout amount and 5% platform fee
        /// 4. Calls PayOS Payout API to transfer funds to seller
        /// 5. Records PayoutTransaction with success/failure status
        /// </summary>
        public async Task ProcessWebhookPayloadAsync(Webhook webhookBody)
        {
            // 1. Verify webhook signature
            WebhookData webhookData;
            try
            {
                webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webhook signature verification failed");
                throw new AppException("Invalid webhook signature", 400, "INVALID_WEBHOOK");
            }

            var orderCode = webhookData.OrderCode;
            _logger.LogInformation("Processing webhook for orderCode: {OrderCode}", orderCode);

            // Find the payment by matching the order code pattern
            // orderCode = timestamp + orderId, we need to extract the orderId
            // Find the payment record that was created during checkout
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Status == "pending" && p.Method == "PayOS" && !p.IsDeleted);

            // Try to match by finding orders and comparing
            var allPendingPayments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Status == "pending" && p.Method == "PayOS" && !p.IsDeleted)
                .ToListAsync();

            // Match by checking if orderCode ends with the orderId
            payment = allPendingPayments
                .FirstOrDefault(p => orderCode.ToString().EndsWith(p.OrderId.ToString()));

            if (payment == null)
            {
                _logger.LogWarning("No pending payment found for orderCode: {OrderCode}", orderCode);
                return;
            }

            var orderId = payment.OrderId;

            // 2. Update Payment and Order status
            payment.Status = "completed";
            payment.PaymentDate = DateTimeOffset.UtcNow;

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for payment webhook", orderId);
                return;
            }

            order.Status = "confirmed";
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            // 3. Find seller and perform payout
            var sellerId = order.OrderItems.FirstOrDefault()?.Product?.SellerId;
            if (!sellerId.HasValue)
            {
                _logger.LogWarning("No seller found for order {OrderId}", orderId);
                return;
            }

            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == sellerId.Value && !sp.IsDeleted);

            if (sellerProfile == null || string.IsNullOrEmpty(sellerProfile.AccountNumber))
            {
                _logger.LogWarning("Seller profile or bank info missing for seller {SellerId}, order {OrderId}", sellerId, orderId);

                // Record a failed payout due to missing bank info
                var failedPayout = new Entities.PayoutTransaction
                {
                    OrderId = orderId,
                    SellerProfileId = sellerProfile?.Id ?? 0,
                    Amount = order.TotalPrice * 0.95m,
                    PlatformFee = order.TotalPrice * 0.05m,
                    BankName = sellerProfile?.BankName ?? "N/A",
                    AccountNumber = sellerProfile?.AccountNumber ?? "N/A",
                    AccountHolderName = sellerProfile?.AccountHolderName ?? "N/A",
                    Status = "failed",
                    Note = "Thông tin ngân hàng của người bán bị thiếu"
                };
                _context.PayoutTransactions.Add(failedPayout);
                await _context.SaveChangesAsync();
                return;
            }

            // 4. Calculate payout amounts
            decimal payoutAmount = order.TotalPrice * 0.95m;
            decimal platformFee = order.TotalPrice * 0.05m;

            // 5. Call PayOS Payout API
            string payoutStatus = "completed";
            string payoutNote = "Chuyển khoản tự động thành công";
            string? transactionId = null;

            try
            {
                var payoutRequest = new PayoutRequest
                {
                    Amount = (long)payoutAmount,
                    ToAccountNumber = sellerProfile.AccountNumber,
                    ToBin = sellerProfile.BankName ?? "",
                    Description = $"Thanh toan don hang #{orderId}",
                    ReferenceId = $"order-{orderId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
                };

                var idempotencyKey = Guid.NewGuid().ToString();
                var payoutResult = await _payOSClient.Payouts.CreateAsync(payoutRequest, idempotencyKey);

                transactionId = payoutResult?.Id;
                _logger.LogInformation("Payout successful for order {OrderId}, payoutId: {PayoutId}", orderId, transactionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Payout API call failed for order {OrderId}. Will record as failed for manual processing.", orderId);
                payoutStatus = "failed";
                payoutNote = $"Lỗi gọi API Payout: {ex.Message}";
            }

            // 6. Record PayoutTransaction
            var payoutTransaction = new Entities.PayoutTransaction
            {
                OrderId = orderId,
                SellerProfileId = sellerProfile.Id,
                Amount = payoutAmount,
                PlatformFee = platformFee,
                BankName = sellerProfile.BankName ?? "",
                AccountNumber = sellerProfile.AccountNumber,
                AccountHolderName = sellerProfile.AccountHolderName ?? "",
                Status = payoutStatus,
                Note = payoutNote,
                TransactionId = transactionId
            };

            _context.PayoutTransactions.Add(payoutTransaction);
            await _context.SaveChangesAsync();
        }
    }
}