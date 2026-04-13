using System;
using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    // Keeps track of actions done in the system
    public class ActivityLog
    {
        // Unique ID for each log entry
        public int Id { get; set; }

        // Account ID is optional since admins may not have one
        public int? AccountId { get; set; }

        // Role of the person, defaults to User
        public string Role { get; set; } = "User";

        // Action taken, like creating a post or banning a user
        [Required]
        public string Action { get; set; } = null!;

        // Extra details about the action if needed
        public string? Details { get; set; }

        // Time when the action happened
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Links the log to the related account
        public Account? Account { get; set; }
    }
}
