using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.AccountManagement
{
    public class IndexModel : AdminPageModel
    {
        private readonly PethubContext _context;

        public IndexModel(PethubContext context)
        {
            _context = context;
        }

        public IList<Account> Account { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "All";

        public async Task OnGetAsync()
        {
            try
            {
                var query = _context.Account.AsQueryable();

                if (StatusFilter == "Active")
                {
                    query = query.Where(a => a.AccountStatus == "Active");
                }
                else if (StatusFilter == "Banned")
                {
                    query = query.Where(a => a.AccountStatus == "Banned");
                }

                Account = await query.ToListAsync();
            }
            catch (Exception)
            {
                Account = new List<Account>();
            }
        }

        public async Task<IActionResult> OnPostToggleBanAsync(int id)
        {
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                if (account.AccountStatus == "Banned")
                {
                    account.AccountStatus = "Active";
                    LogActivity("Unbanned User", $"Admin unbanned {account.Username}");
                    TempData["Success"] = $"User {account.Username} has been unbanned.";
                }
                else
                {
                    account.AccountStatus = "Banned";
                    LogActivity("Banned User", $"Admin banned {account.Username}");
                    TempData["Success"] = $"User {account.Username} has been banned.";
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { StatusFilter });
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