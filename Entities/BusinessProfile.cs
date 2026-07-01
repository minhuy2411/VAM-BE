using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAM.Entities
{
    [Table("business_profiles")]
    public class BusinessProfile : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TaxCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? BusinessLicense { get; set; }

        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }

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
