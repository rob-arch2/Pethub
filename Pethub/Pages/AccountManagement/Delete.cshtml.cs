using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.AccountManagement
{
    public class DeleteModel : AdminPageModel
    {
        private readonly PethubContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(PethubContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Account Account { get; set; } = default!;

        // The password is always stored as a Base64 SHA256 hash — display it as-is.
        public string HashedPassword { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                _logger?.LogDebug("DeleteModel: OnGetAsync() started - Id={Id}", id);

                if (id == null)
                {
                    _logger?.LogWarning("DeleteModel: Id is null");
                    return NotFound();
                }

                var account = await _context.Account.FirstOrDefaultAsync(m => m.Id == id);

                if (account is null)
                {
                    _logger?.LogWarning("DeleteModel: Account not found - Id={Id}", id);
                    return NotFound();
                }

                Account = account;
                HashedPassword = Account.Password;
                _logger?.LogDebug("DeleteModel: Account loaded - Username={Username}", account.Username);

                return Page();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ DeleteModel: Exception in OnGetAsync");
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                _logger?.LogDebug("DeleteModel: OnPostAsync() started - Id={Id}", id);

                if (id == null)
                {
                    _logger?.LogWarning("DeleteModel: Id is null in OnPostAsync");
                    return NotFound();
                }

                var account = await _context.Account.FindAsync(id);

                if (account != null)
                {
                    _logger?.LogDebug("DeleteModel: Deleting account - Username={Username}", account.Username);
                    Account = account;
                    _context.Account.Remove(Account);
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("✓ DeleteModel: Account deleted successfully - Username={Username}", account.Username);
                }
                else
                {
                    _logger?.LogWarning("DeleteModel: Account not found for deletion - Id={Id}", id);
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ DeleteModel: Exception during account deletion");
                return NotFound();
            }
        }
    }
}
