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

        public IndexModel(PethubContext context)
        {
            _context = context;
        }

        public Account Account { get; set; } = default!;

        public List<Post> UserPosts { get; set; } = new List<Post>();

        public async Task<IActionResult> OnGetAsync()
        {
            var id = AccountId ?? 0;

            var account = await _context.Account.FindAsync(id);
            if (account == null)
                return RedirectToPage("/Login");

            Account = account;

            UserPosts = await _context.Post
                .Where(p => p.AccountId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}
