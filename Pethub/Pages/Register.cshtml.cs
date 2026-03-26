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
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(PethubContext context, ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
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
            try
            {
                _logger?.LogDebug("RegisterModel: OnPostAsync() started");

                if (!ModelState.IsValid)
                {
                    _logger?.LogWarning("RegisterModel: ModelState invalid");
                    return Page();
                }

                // Don't allow duplicate emails
                _logger?.LogDebug("RegisterModel: Checking if email exists - {Email}", Email);
                bool emailExists = await _context.Account.AnyAsync(a => a.Email == Email);
                if (emailExists)
                {
                    _logger?.LogWarning("RegisterModel: Email already exists - {Email}", Email);
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return Page();
                }

                // Don't allow duplicate display names either
                _logger?.LogDebug("RegisterModel: Checking if username exists - {FullName}", FullName);
                bool usernameExists = await _context.Account.AnyAsync(a => a.Username == FullName);
                if (usernameExists)
                {
                    _logger?.LogWarning("RegisterModel: Username already exists - {FullName}", FullName);
                    ModelState.AddModelError("FullName", "This name is already taken.");
                    return Page();
                }

                // Block underage registrations
                var today = DateTime.Today;
                int age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                
                _logger?.LogDebug("RegisterModel: Age validation - Age={Age}", age);
                if (age < 18)
                {
                    _logger?.LogWarning("RegisterModel: User underage - Age={Age}", age);
                    ModelState.AddModelError("BirthDate", "You must be at least 18 years old to register.");
                    return Page();
                }

                var account = new Account
                {
                    Username = FullName,
                    Password = HashPassword(Password),
                    Email = Email,
                    Gender = Gender,
                    Address = "Not Specified",
                    Contact = ContactNumber,
                    Birthday = BirthDate,
                    Age = age
                };

                _logger?.LogDebug("RegisterModel: Adding new account to database - Username={Username}", FullName);
                _context.Account.Add(account);
                await _context.SaveChangesAsync();
                
                _logger?.LogInformation("✓ RegisterModel: Registration successful - Username={Username}, Email={Email}", FullName, Email);

                TempData["ToastSuccess"] = "Registration successful! Welcome to PetHub. Please log in.";
                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ RegisterModel: Exception during registration");
                ErrorMessage = "Registration failed. Please try again.";
                return Page();
            }
        }

        // Must match the hashing method in Login so passwords can be compared
        private string HashPassword(string password)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ RegisterModel: Exception during password hashing");
                throw;
            }
        }
    }
}
