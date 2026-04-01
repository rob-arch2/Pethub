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

        // Filter for sorting Active, Dismissed, and Banned user reports
        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "Active";

        public int TotalActiveReports { get; set; }
        public int UniqueUsersReported { get; set; }
        public int TotalBannedUsers { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Report
                .Include(r => r.Post)
                    .ThenInclude(p => p.Account)
                .Include(r => r.Reporter)
                .AsQueryable();

            // Apply Filters
            if (Filter == "Active")
            {
                // Show only active reports where the author is NOT banned
                query = query.Where(r => r.Status == "Active" && r.Post.Account.AccountStatus != "Banned");
            }
            else if (Filter == "Dismissed")
            {
                // Show reports marked as dismissed
                query = query.Where(r => r.Status == "Dismissed");
            }
            else if (Filter == "Banned")
            {
                // Show reports strictly for banned users
                query = query.Where(r => r.Post.Account.AccountStatus == "Banned");
            }

            Reports = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            TotalActiveReports = await _context.Report.CountAsync(r => r.Status == "Active" && r.Post.Account.AccountStatus != "Banned");
            UniqueUsersReported = await _context.Report.Select(r => r.Post.AccountId).Distinct().CountAsync();
            TotalBannedUsers = await _context.Account.CountAsync(a => a.AccountStatus == "Banned");
        }

        public async Task<IActionResult> OnPostDismissAsync(int reportId)
        {
            var report = await _context.Report.Include(r => r.Post).FirstOrDefaultAsync(r => r.Id == reportId);
            if (report != null)
            {
                report.Status = "Dismissed";
                LogActivity("Dismissed Report", $"Report ID {reportId} on Post: {report.Post.Title}");
                await _context.SaveChangesAsync();
                TempData["Success"] = "Report dismissed successfully.";
            }
            return RedirectToPage(new { Filter });
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            var post = await _context.Post.FindAsync(postId);
            if (post != null)
            {
                _context.Post.Remove(post);
                LogActivity("Deleted Post", $"Deleted reported Post ID: {postId}");
                await _context.SaveChangesAsync();
                TempData["Success"] = "Post deleted successfully.";
            }
            return RedirectToPage(new { Filter });
        }

        public async Task<IActionResult> OnPostToggleBanUserAsync(int accountId)
        {
            var account = await _context.Account.FindAsync(accountId);
            if (account != null)
            {
                if (account.AccountStatus == "Banned")
                {
                    account.AccountStatus = "Active";
                    LogActivity("Unbanned User", $"Admin unbanned user: {account.Username}");
                    TempData["Success"] = $"User {account.Username} has been unbanned.";
                }
                else
                {
                    account.AccountStatus = "Banned";
                    LogActivity("Banned User", $"Admin banned user: {account.Username}");
                    TempData["Success"] = $"User {account.Username} has been banned.";
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { Filter });
        }

        private void LogActivity(string action, string details)
        {
            var log = new ActivityLog
            {
                AccountId = null, // Admin action
                Role = "Admin",
                Action = action,
                Details = details,
                Timestamp = DateTime.Now
            };
            _context.ActivityLog.Add(log);
        }
    }
}