using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.UserDashboard
{
    public class MyReportsModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public MyReportsModel(PethubContext context)
        {
            _context = context;
        }

        public IList<Report> Reports { get; set; } = new List<Report>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!CurrentAccountId.HasValue)
                return RedirectToPage("/Login");

            Reports = await _context.Report
                .Include(r => r.Post)
                .Where(r => r.ReporterAccountId == CurrentAccountId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostRetractAsync(int reportId)
        {
            if (!CurrentAccountId.HasValue)
                return RedirectToPage("/Login");

            // Only let the reporter retract their own report
            var report = await _context.Report
                .Include(r => r.Post)
                .FirstOrDefaultAsync(r => r.Id == reportId
                                       && r.ReporterAccountId == CurrentAccountId.Value);

            if (report != null)
            {
                int postId = report.PostId;
                string postTitle = report.Post.Title;

                _context.Report.Remove(report);

                // If no other active reports remain on this post, restore it to Active
                bool hasOtherActiveReports = await _context.Report
                    .AnyAsync(r => r.PostId == postId
                                && r.Id != reportId
                                && r.Status == "Active");

                if (!hasOtherActiveReports)
                {
                    var post = await _context.Post.FindAsync(postId);
                    if (post != null)
                        post.Status = "Active";
                }

                // Log the retraction
                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = CurrentAccountId.Value,
                    Role = "User",
                    Action = "Retracted Report",
                    Details = $"Retracted report on post: '{postTitle}'",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "Your report has been retracted successfully.";
            }

            return RedirectToPage();
        }
    }
}