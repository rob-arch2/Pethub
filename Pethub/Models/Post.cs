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

        // Stored path e.g. /uploads/filename.jpg — nullable since image is optional
        public string? ImagePath { get; set; }

        // Nullable — listing can be free
        [Range(0, 999999, ErrorMessage = "Price must be a positive number.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location must not exceed 200 characters.")]
        public string Location { get; set; }

        // Pending | Approved | Rejected
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key to Account
        public int AccountId { get; set; }
        public Account Account { get; set; }
    }
}