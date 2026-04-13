using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    // Represents a user account in the system
    public class Account
    {
        // Unique ID for each account
        public int Id { get; set; }

        // Username must be 2–100 characters
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Username must be 2–100 characters.")]
        public string Username { get; set; }

        // Password is required and treated as hidden text
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Email must be valid and is required
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        // Gender is required
        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        // Address is required and limited to 200 characters
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address must not exceed 200 characters.")]
        public string Address { get; set; }

        // Contact number must follow PH format
        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Enter a valid PH number (e.g. 09XXXXXXXXX).")]
        public string Contact { get; set; }

        // Birthday is required
        [Required(ErrorMessage = "Birthday is required.")]
        public DateTime Birthday { get; set; }

        // Age is stored as a number
        public int Age { get; set; }

        // Account status defaults to Active and can change to Banned
        public string AccountStatus { get; set; } = "Active";
    }
}
