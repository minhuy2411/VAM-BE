using PayOS.Models.Webhooks;
using VAM.DTOs;

namespace VAM.Services
{
    public interface IPaymentService : IServiceBase<PaymentDto, CreatePaymentDto, UpdatePaymentDto>
    {
        /// <summary>
        /// Creates a PayOS checkout link for the order amount (or optional custom amount from frontend).
        /// Returns the checkout URL.
        /// </summary>
        Task<string> CreateCheckoutUrlAsync(int orderId, decimal? amount = null);

        /// <summary>
        /// Processes the PayOS webhook: verifies signature, updates Payment/Order status,
        /// calculates 95% payout, calls PayOS payout API, and records PayoutTransaction.
        /// </summary>
        Task ProcessWebhookPayloadAsync(Webhook webhookBody);

        /// <summary>
        /// Manually triggers payout for an order (useful for testing or admin manual dispatch).
        /// </summary>
        Task<Entities.PayoutTransaction> ExecuteSellerPayoutAsync(int orderId);
    }
}