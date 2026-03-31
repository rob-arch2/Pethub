using Microsoft.AspNetCore.Mvc;
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

        public IList<Report> Reports { get; set; } = default!;

        // New Moderation Stats
        public int TotalActiveReports { get; set; }
        public int UniqueUsersReported { get; set; }
        public int TotalBannedUsers { get; set; }

        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Reports = await _context.Report
                .Include(r => r.Post)
                    .ThenInclude(p => p.Account)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Calculate stats for the UI
            TotalActiveReports = Reports.Count;
            UniqueUsersReported = Reports.Select(r => r.Post.AccountId).Distinct().Count();
            TotalBannedUsers = await _context.Account.CountAsync(a => a.AccountStatus == "Banned");
        }

        public async Task<IActionResult> OnPostDismissAsync(int reportId)
        {
            var report = await _context.Report.FindAsync(reportId);
            if (report != null)
            {
                _context.Report.Remove(report);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Report dismissed successfully.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            var post = await _context.Post.FindAsync(postId);
            if (post != null)
            {
                _context.Post.Remove(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Post deleted successfully.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBanUserAsync(int accountId)
        {
            var account = await _context.Account.FindAsync(accountId);
            if (account != null)
            {
                account.AccountStatus = "Banned";
                await _context.SaveChangesAsync();
                TempData["Success"] = $"User {account.Username} has been banned.";
            }
            return RedirectToPage();
        }
    }
}