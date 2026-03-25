using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages
{
    public class LandingModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public LandingModel(PethubContext context)
        {
            _context = context;
        }

        public List<Post> Posts { get; set; } = new List<Post>();

        public async Task<IActionResult> OnGetAsync()
        {
            Posts = await _context.Post
                .Include(p => p.Account)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}
