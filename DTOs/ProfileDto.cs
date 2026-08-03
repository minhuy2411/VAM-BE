using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VAM.DTOs
{
    public class SellerProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string? FarmAddress { get; set; }
        public string? AquacultureType { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? Certificate { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? VerifiedAt { get; set; }
        public int? VerifiedById { get; set; }
    }

    public class SellerDetailDto : SellerProfileDto
    {
        public UserDto? SellerInfo { get; set; }
        public System.Collections.Generic.List<ProductDto> Products { get; set; } = new();
    }

    public class CreateSellerProfileDto
    {
        [Required]
        [MaxLength(255)]
        public string FarmName { get; set; } = string.Empty;

        public string? FarmAddress { get; set; }

        [MaxLength(100)]
        public string? AquacultureType { get; set; }

        [Required(ErrorMessage = "Tên ngân hàng là bắt buộc")]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số tài khoản ngân hàng là bắt buộc")]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên chủ tài khoản là bắt buộc")]
        [MaxLength(100)]
        public string AccountHolderName { get; set; } = string.Empty;

        public IFormFile? Certificate { get; set; }

        public string? Note { get; set; }
    }

    public class BusinessProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string? BusinessLicense { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? VerifiedAt { get; set; }
        public int? VerifiedById { get; set; }
    }

    public class CreateBusinessProfileDto
    {
        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TaxCode { get; set; } = string.Empty;

        public IFormFile? BusinessLicense { get; set; }

        public string? Address { get; set; }

        public string? Note { get; set; }
    }

    public class ApproveProfileDto
    {
        [Required]
        public bool IsApproved { get; set; }
        // public string? RejectionReason { get; set; } // The user said "do not save to DB", so just send email, no need for RejectionReason here or maybe yes if we want to send it in email
    }
}
