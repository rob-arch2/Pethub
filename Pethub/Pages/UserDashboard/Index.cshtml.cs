using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.UserDashboard
{
    public class IndexModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(PethubContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Account Account { get; set; } = default!;

        public List<Post> UserPosts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger?.LogDebug("IndexModel: OnGetAsync() started");
                var id = AccountId ?? 0;

                _logger?.LogDebug("IndexModel: Loading account - AccountId={AccountId}", id);
                var account = await _context.Account.FindAsync(id);
                if (account == null)
                {
                    _logger?.LogWarning("IndexModel: Account not found - AccountId={AccountId}", id);
                    return RedirectToPage("/Login");
                }

                Account = account;

                _logger?.LogDebug("IndexModel: Loading user posts - AccountId={AccountId}", id);
                UserPosts = await _context.Post
                    .Where(p => p.AccountId == id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger?.LogDebug("✓ IndexModel: Loaded {PostCount} posts for user", UserPosts.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ IndexModel: Exception in OnGetAsync");
                return RedirectToPage("/Login");
            }
        }
    }
}

