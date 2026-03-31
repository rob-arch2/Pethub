using Microsoft.AspNetCore.Mvc;
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
        public IList<Post> UserPosts { get; set; } = new List<Post>();

        // We still load this so the dashboard can show a quick summary
        public IList<Pet> MyPets { get; set; } = new List<Pet>();

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Load user profile
            Account = await _context.Account.FirstAsync(a => a.Id == CurrentAccountId);

            // 2. Load user's posts
            UserPosts = await _context.Post
                .Where(p => p.AccountId == CurrentAccountId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // 3. Load user's registered pets for the summary view
            MyPets = await _context.Pet
                .Where(p => p.AccountId == CurrentAccountId)
                .ToListAsync();

            return Page();
        }
    }
}