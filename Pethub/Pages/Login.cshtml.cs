using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using System.Security.Cryptography;
using System.Text;

namespace Pethub.Pages
{
    public class LoginModel : PageModel
    {
        private readonly PethubContext _context;

        // Admin credentials
        private const string AdminUsername = "Admin@pethub.com";
        private const string AdminPassword = "admin123";

        public LoginModel(PethubContext context)
        {
            _context = context;
        }

        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Initial load
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ErrorMessage = "Email and password are required.";
                    return Page();
                }

                // Check admin credentials
                if (Email.Trim() == AdminUsername && Password == AdminPassword)
                {
                    if (HttpContext?.Session == null)
                    {
                        ErrorMessage = "Session configuration error. Please contact support.";
                        return Page();
                    }

                    // Setup admin session
                    HttpContext.Session.SetString("AccountRole", "Admin");
                    HttpContext.Session.SetString("AccountUsername", "Admin");
                    HttpContext.Session.SetInt32("AccountId", 0);

                    return RedirectToPage("/Admin/Dashboard");
                }

                // Hash password and find user
                string hashedPassword = HashPassword(Password);
                var account = await _context.Account
                    .FirstOrDefaultAsync(a => a.Email == Email && a.Password == hashedPassword);

                if (account == null)
                {
                    ErrorMessage = "Invalid email or password.";
                    return Page();
                }

                if (HttpContext?.Session == null)
                {
                    ErrorMessage = "Session configuration error. Please contact support.";
                    return Page();
                }

                // Setup user session
                HttpContext.Session.SetInt32("AccountId", account.Id);
                HttpContext.Session.SetString("AccountUsername", account.Username);
                HttpContext.Session.SetString("AccountRole", "User");

                // Check ban status to display the appropriate notification on Landing
                if (account.AccountStatus == "Banned")
                {
                    TempData["BannedPopup"] = "Your account has been banned. You can still browse, but you cannot create new posts.";
                }
                else
                {
                    TempData["ToastSuccess"] = $"Welcome back, {account.Username}!";
                }

                return RedirectToPage("/Landing");
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
        }

        // Hash password to match registration
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}