using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be 3–100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters.")]
        public string Description { get; set; }

        // Stored as /uploads/filename.ext — nullable, image is optional
        public string? ImagePath { get; set; }

        [Range(0, 999999, ErrorMessage = "Price must be a positive number.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location must not exceed 200 characters.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; } // Adoption | Lost Pet | For Sale | Found Pet

        // Active | Reported | Removed
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key to Account
        public int AccountId { get; set; }
        public Account Account { get; set; }

        // Navigation — a post can have many reports
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}