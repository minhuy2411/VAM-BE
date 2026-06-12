using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAM.Entities
{
    [Table("users")]
    public class User : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "active";

        public string? Address { get; set; }

        public virtual SellerProfile? SellerProfile { get; set; }
        public virtual BusinessProfile? BusinessProfile { get; set; }
    }

    public enum UserRole
    {
        admin,
        seller,
        buyer,
        customer,
    }

    public enum UserStatus
    {
        active,
        inactive,
        pending,
        banned,
    }
}
