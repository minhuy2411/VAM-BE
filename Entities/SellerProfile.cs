using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAM.Entities
{
    [Table("seller_profiles")]
    public class SellerProfile : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(255)]
        public string FarmName { get; set; } = string.Empty;

        public string? FarmAddress { get; set; }

        [MaxLength(100)]
        public string? AquacultureType { get; set; }

        [MaxLength(255)]
        public string? Certificate { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; } // e.g. MB, VCB, Agribank (NAPAS code)

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(100)]
        public string? AccountHolderName { get; set; }

        [Required]
        public ProfileStatus Status { get; set; } = ProfileStatus.PENDING;

        public DateTimeOffset? VerifiedAt { get; set; }

        public int? VerifiedById { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("VerifiedById")]
        public virtual User? VerifiedBy { get; set; }
    }
}
