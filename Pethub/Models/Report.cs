using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    // Represents a report made on a post
    public class Report
    {
        // Unique ID for each report
        public int Id { get; set; }

        // Links the report to the post being reported
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        // Links the report to the account that submitted it
        public int ReporterAccountId { get; set; }
        public Account Reporter { get; set; } = null!;

        // Reason category is required, like spam or inappropriate content
        [Required(ErrorMessage = "Please select a reason.")]
        public string ReasonCategory { get; set; } = null!;

        // Optional details about the reason, limited to 500 characters
        [StringLength(500, ErrorMessage = "Detail must not exceed 500 characters.")]
        public string? ReasonDetail { get; set; }

        // Date and time when the report was created
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Status shows if the report is Active or Dismissed
        public string Status { get; set; } = "Active";
    }
}
