using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Admin
{
    public class ReportedPostsModel : AdminPageModel
    {
        private readonly PethubContext _context;

        public ReportedPostsModel(PethubContext context)
        {
            _context = context;
        }

        public List<ReportedPostEntry> ReportedPosts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var posts = await _context.Post
                .Include(p => p.Account)
                .Include(p => p.Reports)
                    .ThenInclude(r => r.Reporter)
                .Where(p => p.Status == "Reported")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ReportedPosts = posts.Select(p => new ReportedPostEntry
            {
                Post = p,
                Reports = p.Reports.OrderByDescending(r => r.CreatedAt).ToList(),
                ReportCount = p.Reports.Count
            }).ToList();
        }

        // Remove: hides the post from the public feed permanently
        public async Task<IActionResult> OnPostRemoveAsync(int postId)
        {
            var post = await _context.Post.FindAsync(postId);
            if (post != null)
            {
                post.Status = "Removed";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Post has been removed from the feed.";
            }
            return RedirectToPage();
        }

        // Dismiss: the post is fine — restore it to Active and clear the flag
        public async Task<IActionResult> OnPostDismissAsync(int postId)
        {
            var post = await _context.Post.FindAsync(postId);
            if (post != null)
            {
                post.Status = "Active";
                // Report records are kept for audit history but the post is restored
                await _context.SaveChangesAsync();
                TempData["Success"] = "Report dismissed. Post is active again.";
            }
            return RedirectToPage();
        }

        // Flat DTO — avoids referencing EF navigation properties directly in the view loop
        public class ReportedPostEntry
        {
            public Post Post { get; set; }
            public List<Report> Reports { get; set; }
            public int ReportCount { get; set; }
        }
    }
}