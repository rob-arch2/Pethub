using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    public class Report
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int ReporterAccountId { get; set; }
        public Account Reporter { get; set; } = null!;

        [Required(ErrorMessage = "Please select a reason.")]
        public string ReasonCategory { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Detail must not exceed 500 characters.")]
        public string? ReasonDetail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Tracks whether the report is Active or Dismissed
        public string Status { get; set; } = "Active";
    }
}