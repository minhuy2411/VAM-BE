using System;
using System.ComponentModel.DataAnnotations;

namespace VAM.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset PaymentDate { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Method { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }
    }

    public class UpdatePaymentDto
    {
        [MaxLength(20)]
        public string? Status { get; set; }
    }
}