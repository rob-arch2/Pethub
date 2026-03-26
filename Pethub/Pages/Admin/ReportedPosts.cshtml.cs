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
        private readonly ILogger<ReportedPostsModel> _logger;

        public ReportedPostsModel(PethubContext context, ILogger<ReportedPostsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<ReportedPostEntry> ReportedPosts { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                _logger?.LogDebug("ReportedPostsModel: OnGetAsync() started");
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

                _logger?.LogDebug("ReportedPostsModel: Loaded {Count} reported posts", ReportedPosts.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ ReportedPostsModel: Exception in OnGetAsync");
                ReportedPosts = new List<ReportedPostEntry>();
            }
        }

        // Remove: hides the post from the public feed permanently
        public async Task<IActionResult> OnPostRemoveAsync(int postId)
        {
            try
            {
                _logger?.LogDebug("ReportedPostsModel: OnPostRemoveAsync() started - PostId={PostId}", postId);
                var post = await _context.Post.FindAsync(postId);
                if (post != null)
                {
                    post.Status = "Removed";
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("✓ ReportedPostsModel: Post removed - PostId={PostId}, Title={Title}", 
                        postId, post.Title);
                    TempData["Success"] = "Post has been removed from the feed.";
                }
                else
                {
                    _logger?.LogWarning("ReportedPostsModel: Post not found for removal - PostId={PostId}", postId);
                }
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ ReportedPostsModel: Exception in OnPostRemoveAsync");
                TempData["Error"] = "An error occurred while removing the post.";
                return RedirectToPage();
            }
        }

        // Dismiss: the post is fine — restore it to Active and clear the flag
        public async Task<IActionResult> OnPostDismissAsync(int postId)
        {
            try
            {
                _logger?.LogDebug("ReportedPostsModel: OnPostDismissAsync() started - PostId={PostId}", postId);
                var post = await _context.Post.FindAsync(postId);
                if (post != null)
                {
                    post.Status = "Active";
                    // Report records are kept for audit history but the post is restored
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("✓ ReportedPostsModel: Report dismissed, post restored - PostId={PostId}, Title={Title}", 
                        postId, post.Title);
                    TempData["Success"] = "Report dismissed. Post is active again.";
                }
                else
                {
                    _logger?.LogWarning("ReportedPostsModel: Post not found for dismissal - PostId={PostId}", postId);
                }
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ ReportedPostsModel: Exception in OnPostDismissAsync");
                TempData["Error"] = "An error occurred while dismissing the report.";
                return RedirectToPage();
            }
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