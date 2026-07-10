using PayOS.Models.Webhooks;
using VAM.DTOs;

namespace VAM.Services
{
    public interface IPaymentService : IServiceBase<PaymentDto, CreatePaymentDto, UpdatePaymentDto>
    {
        /// <summary>
        /// Creates a PayOS checkout link for 100% of the order amount.
        /// Returns the checkout URL.
        /// </summary>
        Task<string> CreateCheckoutUrlAsync(int orderId);

        /// <summary>
        /// Processes the PayOS webhook: verifies signature, updates Payment/Order status,
        /// calculates 95% payout, calls PayOS payout API, and records PayoutTransaction.
        /// </summary>
        Task ProcessWebhookPayloadAsync(Webhook webhookBody);
    }
}