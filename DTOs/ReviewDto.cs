using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VAM.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerAvatar { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string? SellerReply { get; set; }
        public DateTimeOffset? SellerRepliedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public List<IFormFile>? Images { get; set; }
    }

    public class UpdateReviewDto
    {
        [Required]
        public int Id { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public List<IFormFile>? NewImages { get; set; }

        public List<string>? KeepImageUrls { get; set; }
    }

    public class SellerReplyDto
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reply { get; set; } = string.Empty;
    }

    public class ReviewFilterDto
    {
        public int? ProductId { get; set; }
        public int? SellerId { get; set; }
        public int? Rating { get; set; }
        public bool? HasImages { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "newest"; // "newest", "highest_rating", "lowest_rating"
    }

    public class ProductRatingSummaryDto
    {
        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public int WithImagesCount { get; set; }
    }

    public class SellerRatingSummaryDto
    {
        public int SellerId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public double ResponseRate { get; set; } // Percentage of reviews seller has replied to
    }
}