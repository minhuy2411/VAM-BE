using System.ComponentModel.DataAnnotations;

namespace VAM.DTOs
{
    public class FarmDto
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Certificate { get; set; }
    }

    public class CreateFarmDto
    {
        [Required]
        public int SellerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FarmName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Certificate { get; set; }
    }

    public class UpdateFarmDto
    {
        [MaxLength(255)]
        public string? FarmName { get; set; }

        public string? Location { get; set; }

        [MaxLength(255)]
        public string? Certificate { get; set; }
    }
}