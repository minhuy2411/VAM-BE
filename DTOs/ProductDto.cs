using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VAM.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public string? SellerName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? FarmId { get; set; }
        public string? FarmName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        public int SellerId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? FarmId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        public List<IFormFile>? Images { get; set; }
    }

    public class UpdateProductDto
    {
        public int Id { get; set; }

        public int? CategoryId { get; set; }

        public int? FarmId { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public decimal? Quantity { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; }

        public List<IFormFile>? NewImages { get; set; }
        public List<string>? ExistingImageUrls { get; set; }
    }

    public class ProductFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? FarmId { get; set; }
        public string? Location { get; set; }
        public double? MinRating { get; set; }
        public string? Status { get; set; }
    }

    public class ApproveProductDto
    {
        [Required]
        public string Status { get; set; } = "approved"; // "approved" or "rejected"
        public string? Note { get; set; }
    }
}