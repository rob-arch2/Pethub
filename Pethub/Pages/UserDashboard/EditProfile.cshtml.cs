using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.UserDashboard
{
    // Page for editing a user’s profile
    public class EditProfileModel : AuthenticatedPageModel
    {
        // Database context for queries
        private readonly PethubContext _context;
        // Logger for tracking activity
        private readonly ILogger<EditProfileModel> _logger;

        // Constructor sets up database and logger
        public EditProfileModel(PethubContext context, ILogger<EditProfileModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Account object bound to the form
        [BindProperty]
        public Account Account { get; set; } = default!;

        // Loads the current user’s profile for editing
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

        // Handles saving changes to the profile
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger?.LogDebug("EditProfileModel: OnPostAsync() started");

                // Skip email validation since it’s unchanged
                ModelState.Remove("Account.Email");

                // Show errors if validation fails
                if (!ModelState.IsValid)
                {
                    _logger?.LogWarning("EditProfileModel: ModelState invalid");
                    return Page();
                }

                // Prevent editing another user’s profile
                if (Account.Id != (AccountId ?? 0))
                {
                    _logger?.LogWarning("EditProfileModel: User attempted to edit another user's profile - AccountId={CurrentId}, TargetId={TargetId}",
                        AccountId, Account.Id);
                    return Forbid();
                }

                // Recalculate age based on birthday
                var today = DateTime.Today;
                int age = today.Year - Account.Birthday.Year;
                if (Account.Birthday.Date > today.AddYears(-age)) age--;
                Account.Age = age;

                _logger?.LogDebug("EditProfileModel: Updating account - Username={Username}, Age={Age}",
                    Account.Username, Account.Age);

                // Log the profile edit
                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = Account.Id,
                    Role = "User",
                    Action = "Edited Profile",
                    Details = $"Updated profile details for '{Account.Username}'",
                    Timestamp = DateTime.Now
                });

                // Mark account as modified
                _context.Attach(Account).State = EntityState.Modified;

                try
                {
                    // Save changes to database
                    await _context.SaveChangesAsync();
                    _logger?.LogDebug("EditProfileModel: Account saved to database successfully");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger?.LogError(ex, "❌ EditProfileModel: Concurrency error while saving account");
                    return NotFound();
                }

                // Update session username if possible
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
