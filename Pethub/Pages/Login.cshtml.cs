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

        private string AdminUsername = "Admin@pethub.com";
        private string AdminPassword = "admin123";

        public LoginModel(PethubContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Email and password are required.";
                return Page();
            }

            // Hardcoded admin check — not stored in the database
            if (Email.Trim() == AdminUsername && Password == AdminPassword)
            {
                HttpContext.Session.SetString("AccountRole", "Admin");
                HttpContext.Session.SetString("AccountUsername", AdminUsername);
                HttpContext.Session.SetInt32("AccountId", 0);
                return RedirectToPage("/Admin/Dashboard");
            }

            // Regular user check
            string hashedPassword = HashPassword(Password);

            var account = await _context.Account
                .FirstOrDefaultAsync(a => a.Email == Email && a.Password == hashedPassword);

            if (account == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            HttpContext.Session.SetInt32("AccountId", account.Id);
            HttpContext.Session.SetString("AccountUsername", account.Username);
            HttpContext.Session.SetString("AccountRole", "User");

            TempData["ToastSuccess"] = $"Welcome back, {account.Username}!";

            return RedirectToPage("/Landing");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}