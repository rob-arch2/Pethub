using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Admin
{
    public class DashboardModel : AdminPageModel
    {
        // Admin dashboard page model - prepares counts, percentages and recent accounts for the view
        private readonly PethubContext _context;

        public DashboardModel(PethubContext context)
        {
            _context = context;
        }

        // Summary counts
        public int TotalAccounts { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }

        // Age group breakdown
        public int AgeGroup1 { get; set; } // 18–25
        public int AgeGroup2 { get; set; } // 26–35
        public int AgeGroup3 { get; set; } // 36–50
        public int AgeGroup4 { get; set; } // 51+

        // Pre-calculated percentages — avoids Razor scope issues
        public double MalePercent { get; set; }
        public double FemalePercent { get; set; }
        public double Ag1p { get; set; }
        public double Ag2p { get; set; }
        public double Ag3p { get; set; }
        public double Ag4p { get; set; }

        // Highlights
        public string NewestAccount { get; set; } = "—";
        public string OldestAccount { get; set; } = "—";
        public string YoungestAccount { get; set; } = "—";
        public string OldestByAge { get; set; } = "—";

        // Recent registrations (last 5)
        public List<RecentEntry> RecentAccounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var accounts = await _context.Account.ToListAsync();

            TotalAccounts = accounts.Count;
            MaleCount = accounts.Count(a => a.Gender == "Male");
            FemaleCount = accounts.Count(a => a.Gender == "Female");

            AgeGroup1 = accounts.Count(a => a.Age >= 18 && a.Age <= 25);
            AgeGroup2 = accounts.Count(a => a.Age >= 26 && a.Age <= 35);
            AgeGroup3 = accounts.Count(a => a.Age >= 36 && a.Age <= 50);
            AgeGroup4 = accounts.Count(a => a.Age >= 51);

            if (TotalAccounts > 0)
            {
                MalePercent = Math.Round((double)MaleCount / TotalAccounts * 100, 1);
                FemalePercent = Math.Round((double)FemaleCount / TotalAccounts * 100, 1);
                Ag1p = Math.Round((double)AgeGroup1 / TotalAccounts * 100, 1);
                Ag2p = Math.Round((double)AgeGroup2 / TotalAccounts * 100, 1);
                Ag3p = Math.Round((double)AgeGroup3 / TotalAccounts * 100, 1);
                Ag4p = Math.Round((double)AgeGroup4 / TotalAccounts * 100, 1);
            }

            if (accounts.Any())
            {
                var newest = accounts.OrderByDescending(a => a.Birthday).First();
                var oldest = accounts.OrderBy(a => a.Birthday).First();
                var youngest = accounts.OrderBy(a => a.Age).First();
                var oldestAge = accounts.OrderByDescending(a => a.Age).First();

                NewestAccount = $"{newest.Username} ({newest.Birthday:MMM dd, yyyy})";
                OldestAccount = $"{oldest.Username} ({oldest.Birthday:MMM dd, yyyy})";
                YoungestAccount = $"{youngest.Username} — {youngest.Age} yrs old";
                OldestByAge = $"{oldestAge.Username} — {oldestAge.Age} yrs old";

                RecentAccounts = accounts
                    .OrderByDescending(a => a.Id)
                    .Take(5)
                    .Select(a => new RecentEntry
                    {
                        Username = a.Username,
                        Email = a.Email,
                        Gender = a.Gender,
                        Age = a.Age
                    })
                    .ToList();
            }
        }

        public class RecentEntry
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Gender { get; set; }
            public int Age { get; set; }
        }
    }
}