using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    // Represents a post created by a user
    public class Post
    {
        // Unique ID for each post
        public int Id { get; set; }

        // Title is required and must be 3–100 characters
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be 3–100 characters.")]
        public string Title { get; set; } = null!;

        // Description is required and must be at least 10 characters
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters.")]
        public string Description { get; set; } = null!;

        // Path to an image, limited to 2048 characters
        [MaxLength(2048)]
        public string? ImagePath { get; set; }

        // Price must be a positive number
        [Range(0, 999999, ErrorMessage = "Price must be a positive number.")]
        public decimal? Price { get; set; }

        // Location is required and limited to 200 characters
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location must not exceed 200 characters.")]
        public string Location { get; set; } = null!;

        // Category is required, like Adoption or Lost Pet
        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; } = null!;

        // Status defaults to Active
        public string Status { get; set; } = "Active";

        // Date and time when the post was created
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Links the post to the account that created it
        public int AccountId { get; set; }

        // Account reference managed by EF
        public Account Account { get; set; } = null!;

        // A post can have many reports
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
