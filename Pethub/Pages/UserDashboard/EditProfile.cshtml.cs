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
        private readonly ILogger<EditProfileModel> _logger;

        public EditProfileModel(PethubContext context, ILogger<EditProfileModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger?.LogDebug("EditProfileModel: OnGetAsync() started");
                var id = AccountId ?? 0;

                var account = await _context.Account.FindAsync(id);
                if (account == null)
                {
                    _logger?.LogWarning("EditProfileModel: Account not found for ID={AccountId}", id);
                    return RedirectToPage("/Login");
                }

                Account = account;
                _logger?.LogDebug("EditProfileModel: Account loaded - Username={Username}", account.Username);
                return Page();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ EditProfileModel: Exception in OnGetAsync");
                return RedirectToPage("/Login");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger?.LogDebug("EditProfileModel: OnPostAsync() started");

                // Email is displayed read-only — not in the form POST, so exclude from validation
                ModelState.Remove("Account.Email");

                if (!ModelState.IsValid)
                {
                    _logger?.LogWarning("EditProfileModel: ModelState invalid");
                    return Page();
                }

                // Guard: users can only edit their own profile
                if (Account.Id != (AccountId ?? 0))
                {
                    _logger?.LogWarning("EditProfileModel: User attempted to edit another user's profile - AccountId={CurrentId}, TargetId={TargetId}", 
                        AccountId, Account.Id);
                    return Forbid();
                }

                // Recalculate age from the updated birthday
                var today = DateTime.Today;
                int age = today.Year - Account.Birthday.Year;
                if (Account.Birthday.Date > today.AddYears(-age)) age--;
                Account.Age = age;

                _logger?.LogDebug("EditProfileModel: Updating account - Username={Username}, Age={Age}", 
                    Account.Username, Account.Age);

                _context.Attach(Account).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger?.LogDebug("EditProfileModel: Account saved to database successfully");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger?.LogError(ex, "❌ EditProfileModel: Concurrency error while saving account");
                    return NotFound();
                }

                // Keep the session username in sync if the display name changed
                // Guard: check if HttpContext and Session exist before writing
                if (HttpContext?.Session != null)
                {
                    try
                    {
                        HttpContext.Session.SetString("AccountUsername", Account.Username);
                        _logger?.LogDebug("EditProfileModel: Session username updated - {Username}", Account.Username);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "❌ EditProfileModel: Failed to update session username");
                        // Don't return error - profile was saved successfully
                    }
                }
                else
                {
                    _logger?.LogWarning("EditProfileModel: HttpContext or Session is null, cannot update session username");
                }

                TempData["Success"] = "Profile updated successfully.";
                _logger?.LogInformation("✓ EditProfileModel: Profile updated successfully");
                return RedirectToPage("/UserDashboard/Index");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ EditProfileModel: Unhandled exception in OnPostAsync");
                return Page();
            }
        }
    }
}