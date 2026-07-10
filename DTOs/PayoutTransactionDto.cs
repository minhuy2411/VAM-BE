namespace VAM.DTOs
{
    public class PayoutTransactionDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int SellerProfileId { get; set; }
        public decimal Amount { get; set; }
        public decimal PlatformFee { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? TransactionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
