using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Profile
{
    public class ViewModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public ViewModel(PethubContext context)
        {
            _context = context;
        }

        public Account Author { get; set; } = default!;
        public List<Post> AuthorPosts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var account = await _context.Account.FindAsync(id);
            if (account == null) return NotFound();

            Author = account;

            // Only show active posts from this author
            AuthorPosts = await _context.Post
                .Where(p => p.AccountId == id && p.Status == "Active")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}