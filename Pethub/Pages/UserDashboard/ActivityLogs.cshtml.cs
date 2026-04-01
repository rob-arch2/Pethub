using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.UserDashboard
{
    public class ActivityLogsModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public ActivityLogsModel(PethubContext context)
        {
            _context = context;
        }

        public IList<ActivityLog> Logs { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!CurrentAccountId.HasValue) return RedirectToPage("/Login");

            // Fetch only logs belonging to this specific user
            Logs = await _context.ActivityLog
                .Where(a => a.AccountId == CurrentAccountId.Value)
                .OrderByDescending(a => a.Timestamp)
                .Take(100)
                .ToListAsync();

            return Page();
        }
    }
}