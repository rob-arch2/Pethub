using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Admin
{
    public class NewsfeedModel : AdminPageModel
    {
        private readonly PethubContext _context;
        private readonly IWebHostEnvironment _env;

        public NewsfeedModel(PethubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IList<Post> Posts { get; set; } = new List<Post>();

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        public int TotalPosts { get; set; }
        public int ActivePosts { get; set; }
        public int ReportedPosts { get; set; }
        public int RemovedPosts { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Post
                .Include(p => p.Account)
                .AsQueryable();

            // Category filter
            if (CategoryFilter != "All")
                query = query.Where(p => p.Category == CategoryFilter);

            // Status filter
            if (StatusFilter != "All")
                query = query.Where(p => p.Status == StatusFilter);

            // Search
            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(p =>
                    p.Title.Contains(Search) ||
                    p.Description.Contains(Search) ||
                    p.Account.Username.Contains(Search));

            Posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            // Summary counts (always unfiltered)
            TotalPosts = await _context.Post.CountAsync();
            ActivePosts = await _context.Post.CountAsync(p => p.Status == "Active");
            ReportedPosts = await _context.Post.CountAsync(p => p.Status == "Reported");
            RemovedPosts = await _context.Post.CountAsync(p => p.Status == "Removed");
        }

        // ── Censor: marks post as Removed without deleting it ────────
        public async Task<IActionResult> OnPostCensorAsync(int postId)
        {
            var post = await _context.Post
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post != null)
            {
                post.Status = "Removed";

                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = null,
                    Role = "Admin",
                    Action = "Censored Post",
                    Details = $"Admin censored post '{post.Title}' by {post.Account.Username}",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Post '{post.Title}' has been censored.";
            }

            return RedirectToPage(new { CategoryFilter, StatusFilter, Search });
        }

        // ── Restore: brings a censored post back to Active ────────────
        public async Task<IActionResult> OnPostRestoreAsync(int postId)
        {
            var post = await _context.Post
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post != null)
            {
                post.Status = "Active";

                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = null,
                    Role = "Admin",
                    Action = "Restored Post",
                    Details = $"Admin restored post '{post.Title}' by {post.Account.Username}",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Post '{post.Title}' has been restored to Active.";
            }

            return RedirectToPage(new { CategoryFilter, StatusFilter, Search });
        }

        // ── Hard delete: permanently removes the post ─────────────────
        public async Task<IActionResult> OnPostDeleteAsync(int postId)
        {
            var post = await _context.Post
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post != null)
            {
                // Delete physical image file if it exists
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var webRoot = _env.WebRootPath
                                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var fullPath = Path.Combine(webRoot, post.ImagePath.TrimStart('/'))
                                      .Replace('/', Path.DirectorySeparatorChar);

                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                string title = post.Title;
                string username = post.Account.Username;

                _context.Post.Remove(post);

                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = null,
                    Role = "Admin",
                    Action = "Deleted Post",
                    Details = $"Admin permanently deleted post '{title}' by {username}",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Post '{title}' has been permanently deleted.";
            }

            return RedirectToPage(new { CategoryFilter, StatusFilter, Search });
        }
    }
}