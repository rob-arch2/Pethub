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
        private readonly ILogger<LoginModel> _logger;

        // Admin credentials are hardcoded — the admin account doesn't exist in the database
        private const string AdminUsername = "Admin@pethub.com";
        private const string AdminPassword = "admin123";

        public LoginModel(PethubContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Email and password are required.";
                return Page();
            }

            // Check admin first before hitting the database
            if (Email.Trim() == AdminUsername && Password == AdminPassword)
            {
                HttpContext.Session.SetString("AccountRole", "Admin");
                HttpContext.Session.SetString("AccountUsername", "Admin");
                HttpContext.Session.SetInt32("AccountId", 0); // 0 is the admin sentinel value
                return RedirectToPage("/Admin/Dashboard");
            }

            // Hash what the user typed, then compare against the stored hash
            string hashedPassword = HashPassword(Password);

            var account = await _context.Account
                .FirstOrDefaultAsync(a => a.Email == Email && a.Password == hashedPassword);

            if (account == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Store just enough in the session to identify the user on every page
            HttpContext.Session.SetInt32("AccountId", account.Id);
            HttpContext.Session.SetString("AccountUsername", account.Username);
            HttpContext.Session.SetString("AccountRole", "User");

            TempData["ToastSuccess"] = $"Welcome back, {account.Username}!";
            return RedirectToPage("/Landing");
        }

        // SHA256 hash — must match how passwords are stored during registration
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}