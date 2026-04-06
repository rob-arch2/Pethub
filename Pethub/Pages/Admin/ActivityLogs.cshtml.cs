using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Admin
{
    public class ActivityLogsModel : AdminPageModel
    {
        private readonly PethubContext _context;

        public ActivityLogsModel(PethubContext context)
        {
            _context = context;
        }

        public IList<ActivityLog> Logs { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Fetch the 200 most recent activity logs for performance
            Logs = await _context.ActivityLog
                .Include(a => a.Account)
                .Where(a => a.Role == "Admin")
                .OrderByDescending(a => a.Timestamp)
                .Take(200)
                .ToListAsync();
        }
    }
}