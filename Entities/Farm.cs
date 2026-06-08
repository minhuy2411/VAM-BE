using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAM.Entities
{
    [Table("farms")]
    public class Farm : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SellerId { get; set; }
        
        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        [Required]
        [MaxLength(255)]
        public string FarmName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Certificate { get; set; }
    }
}
