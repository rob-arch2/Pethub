using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.UserDashboard
{
    public class EditProfileModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public EditProfileModel(PethubContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var id = AccountId ?? 0;

            var account = await _context.Account.FindAsync(id);
            if (account == null)
                return RedirectToPage("/Login");

            Account = account;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Email is displayed read-only — not in the form POST, so exclude from validation
            ModelState.Remove("Account.Email");

            if (!ModelState.IsValid)
                return Page();

            // Guard: users can only edit their own profile
            if (Account.Id != (AccountId ?? 0))
                return Forbid();

            // Recalculate age from the updated birthday
            var today = DateTime.Today;
            int age = today.Year - Account.Birthday.Year;
            if (Account.Birthday.Date > today.AddYears(-age)) age--;
            Account.Age = age;

            _context.Attach(Account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            // Keep the session username in sync if the display name changed
            HttpContext.Session.SetString("AccountUsername", Account.Username);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToPage("/UserDashboard/Index");
        }
    }
}