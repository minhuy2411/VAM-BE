using System.ComponentModel.DataAnnotations;

namespace VAM.DTOs
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrls { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public decimal Quantity { get; set; }
    }

    public class UpdateOrderItemDto
    {
        public decimal? Quantity { get; set; }
    }
}