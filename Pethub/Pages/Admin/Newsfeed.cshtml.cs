using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Admin
{
    // Admin page for managing posts in the newsfeed
    public class NewsfeedModel : AdminPageModel
    {
        // Database context for queries
        private readonly PethubContext _context;
        // Environment for handling files
        private readonly IWebHostEnvironment _env;

        // Constructor sets up database and environment
        public NewsfeedModel(PethubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // List of posts to display
        public IList<Post> Posts { get; set; } = new List<Post>();

        // Filter for category
        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; } = "All";

        // Filter for status
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "All";

        // Search keyword
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        // Summary counts for posts
        public int TotalPosts { get; set; }
        public int ActivePosts { get; set; }
        public int ReportedPosts { get; set; }
        public int RemovedPosts { get; set; }

        // Loads posts with filters and summary counts
        public async Task OnGetAsync()
        {
            var query = _context.Post
                .Include(p => p.Account)
                .AsQueryable();

            // Apply category filter
            if (CategoryFilter != "All")
                query = query.Where(p => p.Category == CategoryFilter);

            // Apply status filter
            if (StatusFilter != "All")
                query = query.Where(p => p.Status == StatusFilter);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(p =>
                    p.Title.Contains(Search) ||
                    p.Description.Contains(Search) ||
                    p.Account.Username.Contains(Search));

            // Get filtered posts
            Posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            // Get summary counts without filters
            TotalPosts = await _context.Post.CountAsync();
            ActivePosts = await _context.Post.CountAsync(p => p.Status == "Active");
            ReportedPosts = await _context.Post.CountAsync(p => p.Status == "Reported");
            RemovedPosts = await _context.Post.CountAsync(p => p.Status == "Removed");
        }

        // Marks a post as Removed without deleting it
        public async Task<IActionResult> OnPostCensorAsync(int postId)
        {
            var post = await _context.Post
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post != null)
            {
                post.Status = "Removed";

                // Log the censor action
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

        // Restores a censored post back to Active
        public async Task<IActionResult> OnPostRestoreAsync(int postId)
        {
            var post = await _context.Post
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post != null)
            {
                post.Status = "Active";

                // Log the restore action
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

        // Permanently deletes a post and its image file
        public async Task<IActionResult> OnPostDeleteAsync(int postId)
        {
            var post = await _context.Post
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post != null)
            {
                // Delete image file if it exists
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

                // Remove post from database
                _context.Post.Remove(post);

                // Log the delete action
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
