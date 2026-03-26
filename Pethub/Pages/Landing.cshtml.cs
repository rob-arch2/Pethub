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
        private readonly ILogger<LandingModel> _logger;

        public LandingModel(PethubContext context, ILogger<LandingModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Post> Posts { get; set; } = new List<Post>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogDebug("LandingModel.OnGetAsync() started - CurrentAccountId={AccountId}", CurrentAccountId);

                // Only show Active and Reported posts — Removed posts are hidden from the public feed
                Posts = await _context.Post
                    .Include(p => p.Account)
                    .Where(p => p.Status != "Removed")
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug("✓ Loaded {PostCount} posts", Posts.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception in LandingModel.OnGetAsync()");
                throw;
            }
        }

        // Handles the report modal form — asp-page-handler="Report"
        public async Task<IActionResult> OnPostReportAsync(
            int postId,
            string reasonCategory,
            string? reasonDetail)
        {
            try
            {
                _logger.LogDebug("OnPostReportAsync() started - PostId={PostId}, Reason={Reason}", postId, reasonCategory);

                var reporterId = CurrentAccountId ?? 0;
                _logger.LogDebug("ReporterId from CurrentAccountId={ReporterId}", reporterId);

                var post = await _context.Post.FindAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning("Post not found: PostId={PostId}", postId);
                    return NotFound();
                }

                // Users cannot report their own posts
                if (post.AccountId == reporterId)
                {
                    _logger.LogWarning("User tried to report own post - PostId={PostId}, ReporterId={ReporterId}", 
                        postId, reporterId);
                    TempData["Error"] = "You cannot report your own post.";
                    return RedirectToPage();
                }

                // Each user can only report a given post once
                bool alreadyReported = await _context.Report
                    .AnyAsync(r => r.PostId == postId && r.ReporterAccountId == reporterId);

                if (alreadyReported)
                {
                    _logger.LogWarning("User already reported this post - PostId={PostId}, ReporterId={ReporterId}", 
                        postId, reporterId);
                    TempData["Error"] = "You have already reported this post.";
                    return RedirectToPage();
                }

                // Validate reason was provided
                if (string.IsNullOrWhiteSpace(reasonCategory))
                {
                    _logger.LogWarning("Report submitted without reason");
                    TempData["Error"] = "Please select a reason before submitting.";
                    return RedirectToPage();
                }

                // Create the report record
                var report = new Report
                {
                    PostId = postId,
                    ReporterAccountId = reporterId,
                    ReasonCategory = reasonCategory,
                    ReasonDetail = string.IsNullOrWhiteSpace(reasonDetail)
                                            ? null
                                            : reasonDetail.Trim(),
                    CreatedAt = DateTime.Now
                };
                _context.Report.Add(report);

                // Flag the post so it appears in the admin Reported feed
                post.Status = "Reported";

                await _context.SaveChangesAsync();

                _logger.LogInformation("✓ Post reported successfully - PostId={PostId}, ReporterId={ReporterId}", 
                    postId, reporterId);
                TempData["Success"] = "Post reported. Our team will review it shortly.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception in OnPostReportAsync()");
                TempData["Error"] = "An error occurred while reporting the post.";
                return RedirectToPage();
            }
        }
    }
}