using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    public class Report
    {
        public int Id { get; set; }

        // Which post was reported
        public int PostId { get; set; }

        // = null! — EF populates this at runtime via FK
        public Post Post { get; set; } = null!;

        // Who submitted the report
        public int ReporterAccountId { get; set; }

        // = null! — EF populates this at runtime via FK
        public Account Reporter { get; set; } = null!;

        [Required(ErrorMessage = "Please select a reason.")]
        public string ReasonCategory { get; set; } = null!; // Spam | Abuse | Scam | Inappropriate

        [StringLength(500, ErrorMessage = "Detail must not exceed 500 characters.")]
        public string? ReasonDetail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}