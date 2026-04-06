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

            if (Filter == "Active")
                query = query.Where(r => r.Status == "Active" && r.Post.Account.AccountStatus != "Banned");
            else if (Filter == "Dismissed")
                query = query.Where(r => r.Status == "Dismissed");
            else if (Filter == "Banned")
                query = query.Where(r => r.Post.Account.AccountStatus == "Banned");

            Reports = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            TotalActiveReports = await _context.Report
                .CountAsync(r => r.Status == "Active" && r.Post.Account.AccountStatus != "Banned");
            UniqueUsersReported = await _context.Report
                .Select(r => r.Post.AccountId).Distinct().CountAsync();
            TotalBannedUsers = await _context.Account
                .CountAsync(a => a.AccountStatus == "Banned");
        }

        public async Task<IActionResult> OnPostDismissAsync(int reportId)
        {
            var report = await _context.Report
                .Include(r => r.Post)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report != null)
            {
                report.Status = "Dismissed";

                // Check if any OTHER active reports still exist on this post
                bool hasOtherActiveReports = await _context.Report
                    .AnyAsync(r => r.PostId == report.PostId
                                && r.Id != reportId
                                && r.Status == "Active");

                // If no remaining active reports, reset the post back to Active
                if (!hasOtherActiveReports)
                    report.Post.Status = "Active";

                LogActivity("Dismissed Report",
                    $"Dismissed report ID {reportId} on post: '{report.Post.Title}'");

                await _context.SaveChangesAsync();
                TempData["Success"] = "Report dismissed. Post status restored if no other reports remain.";
            }

            return RedirectToPage(new { Filter });
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            var post = await _context.Post.FindAsync(postId);
            if (post != null)
            {
                _context.Post.Remove(post);
                LogActivity("Deleted Post", $"Deleted reported post ID: {postId} titled '{post.Title}'");
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
            _context.ActivityLog.Add(new ActivityLog
            {
                AccountId = null,
                Role = "Admin",
                Action = action,
                Details = details,
                Timestamp = DateTime.Now
            });
        }
    }
}