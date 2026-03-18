using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Pethub
{
    public class RegisterModel : PageModel
    {
        private readonly PethubContext _context;

        public RegisterModel(PethubContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be at least 2 characters.")]
        public string FullName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Enter a valid PH number (e.g. 09XXXXXXXXX).")]
        public string ContactNumber { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Birthday is required.")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must have uppercase, lowercase, and a number.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            bool emailExists = await _context.Account.AnyAsync(a => a.Email == Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return Page();
            }

            bool usernameExists = await _context.Account.AnyAsync(a => a.Username == FullName);
            if (usernameExists)
            {
                ModelState.AddModelError("FullName", "This name is already taken.");
                return Page();
            }

            var today = DateTime.Today;
            int age = today.Year - BirthDate.Year;
            if (BirthDate.Date > today.AddYears(-age)) age--;
            if (age < 18)
            {
                ModelState.AddModelError("BirthDate", "You must be at least 18 years old to register.");
                return Page();
            }

            string hashedPassword = HashPassword(Password);

            var account = new Account
            {
                Username = FullName,
                Password = hashedPassword,
                Email = Email,
                Gender = Gender,
                Address = "Not Specified",
                Contact = ContactNumber,
                Birthday = BirthDate,
                Age = age
            };

            _context.Account.Add(account);
            await _context.SaveChangesAsync();

            TempData["ToastSuccess"] = "Registration successful! Welcome to PetHub. Please log in.";

            return RedirectToPage("/Login");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}