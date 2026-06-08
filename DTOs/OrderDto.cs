using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VAM.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset OrderDate { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class CreateOrderDto
    {
        [Required]
        public int BuyerId { get; set; }

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public ICollection<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
    }

    public class UpdateOrderDto
    {
        [MaxLength(20)]
        public string? Status { get; set; }
        
        public string? ShippingAddress { get; set; }
    }
}