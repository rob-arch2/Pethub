using Microsoft.AspNetCore.Mvc;
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
                _logger?.LogError(ex, "DeleteModel: Exception in OnGetAsync");
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            int id = Account?.Id ?? 0;

            try
            {
                _logger?.LogDebug("DeleteModel: OnPostAsync() started - Id={Id}", id);

                if (id == 0)
                {
                    _logger?.LogWarning("DeleteModel: Id is 0 or missing in OnPostAsync");
                    return NotFound();
                }

                var account = await _context.Account.FindAsync(id);

                if (account == null)
                {
                    _logger?.LogWarning("DeleteModel: Account not found for deletion - Id={Id}", id);
                    return RedirectToPage("./Index");
                }

                // 1. Delete reports this user FILED on other posts (ReporterAccountId → Restrict)
                var reportsFiled = _context.Report.Where(r => r.ReporterAccountId == id);
                _context.Report.RemoveRange(reportsFiled);

                // 2. Delete reports ON this user's posts, then delete the posts (Post → Account is Restrict)
                var userPosts = _context.Post.Where(p => p.AccountId == id);
                foreach (var post in userPosts)
                {
                    var reportsOnPost = _context.Report.Where(r => r.PostId == post.Id);
                    _context.Report.RemoveRange(reportsOnPost);
                }
                _context.Post.RemoveRange(userPosts);

                // 3. Nullify ActivityLog entries to preserve log history
                var logs = _context.ActivityLog.Where(a => a.AccountId == id);
                foreach (var log in logs)
                    log.AccountId = null;

                // 4. Log the admin action before removing the account
                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = null,
                    Role = "Admin",
                    Action = "Deleted Account",
                    Details = $"Permanently deleted account '{account.Username}' (ID: {account.Id})",
                    Timestamp = DateTime.Now
                });

                // 5. Delete the account (Pet cascades automatically)
                _context.Account.Remove(account);

                await _context.SaveChangesAsync();

                _logger?.LogInformation("DeleteModel: Account deleted successfully - Username={Username}", account.Username);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "DeleteModel: Exception during account deletion");
                return NotFound();
            }
        }
    }
}