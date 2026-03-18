using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Username must be 2–100 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address must not exceed 200 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Enter a valid PH number (e.g. 09XXXXXXXXX).")]
        public string Contact { get; set; }

        [Required(ErrorMessage = "Birthday is required.")]
        public DateTime Birthday { get; set; }

        public int Age { get; set; }
    }
}