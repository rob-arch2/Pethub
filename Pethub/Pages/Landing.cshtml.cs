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

                // Include ALL statuses now — Removed posts show as censored on the feed
                Posts = await _context.Post
                    .Include(p => p.Account)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug("✓ Loaded {PostCount} posts", Posts.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "  Exception in LandingModel.OnGetAsync()");
                throw;
            }
        }

        public async Task<IActionResult> OnPostReportAsync(
            int postId,
            string reasonCategory,
            string? reasonDetail)
        {
            try
            {
                _logger.LogDebug("OnPostReportAsync() started - PostId={PostId}, Reason={Reason}", postId, reasonCategory);

                var reporterId = CurrentAccountId ?? 0;

                var post = await _context.Post.FindAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning("Post not found: PostId={PostId}", postId);
                    return NotFound();
                }

                if (post.AccountId == reporterId)
                {
                    TempData["Error"] = "You cannot report your own post.";
                    return RedirectToPage();
                }

                bool alreadyReported = await _context.Report
                    .AnyAsync(r => r.PostId == postId && r.ReporterAccountId == reporterId);

                if (alreadyReported)
                {
                    TempData["Error"] = "You have already reported this post.";
                    return RedirectToPage();
                }

                if (string.IsNullOrWhiteSpace(reasonCategory))
                {
                    TempData["Error"] = "Please select a reason before submitting.";
                    return RedirectToPage();
                }

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

                post.Status = "Reported";

                await _context.SaveChangesAsync();

                TempData["Success"] = "Post reported. Our team will review it shortly.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in OnPostReportAsync()");
                TempData["Error"] = "An error occurred while reporting the post.";
                return RedirectToPage();
            }
        }
    }
}