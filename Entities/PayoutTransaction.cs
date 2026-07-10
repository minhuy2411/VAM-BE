using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAM.Entities
{
    [Table("payout_transactions")]
    public class PayoutTransaction : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        public int SellerProfileId { get; set; }

        [ForeignKey("SellerProfileId")]
        public SellerProfile? SellerProfile { get; set; }

        [Required]
        [Column(TypeName = "decimal(15, 2)")]
        public decimal Amount { get; set; } // 95% payout

        [Required]
        [Column(TypeName = "decimal(15, 2)")]
        public decimal PlatformFee { get; set; } // 5% fee

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountHolderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // completed, failed, pending

        public string? Note { get; set; }

        public string? TransactionId { get; set; }
    }
}
