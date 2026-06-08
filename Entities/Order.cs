using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace VAM.Entities
{
    [Table("orders")]
    public class Order : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        [Required]
        [Column(TypeName = "decimal(15, 2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "pending";

        [Required]
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
