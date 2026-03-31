using System;
using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        // Nullable because Admins might not have a standard AccountId
        public int? AccountId { get; set; }

        public string Role { get; set; } = "User"; // "Admin" or "User"

        [Required]
        public string Action { get; set; } = null!; // e.g., "Created Post", "Banned User"

        public string? Details { get; set; } // Extra info like "Post Title: Fluffy"

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public Account? Account { get; set; }
    }
}