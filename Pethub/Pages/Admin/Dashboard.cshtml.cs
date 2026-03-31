using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;
using System.Text.Json;

namespace Pethub.Pages.Admin
{
    public class DashboardModel : AdminPageModel
    {
        private readonly PethubContext _context;

        public DashboardModel(PethubContext context)
        {
            _context = context;
        }

        // Post Velocity
        public int Posts1h { get; set; }
        public int Posts12h { get; set; }
        public int Posts1d { get; set; }
        public int Posts7d { get; set; }

        // Chart Data
        public string ChartLabelsJson { get; set; } = "[]";
        public string ChartDataJson { get; set; } = "[]";

        // Category / Ratios
        public int TotalLost { get; set; }
        public int TotalFound { get; set; }
        public int TotalAdoptions { get; set; }

        // Recent Additions
        public IList<Account> RecentAccounts { get; set; } = new List<Account>();
        public IList<Pet> RecentPets { get; set; } = new List<Pet>();

        public async Task<IActionResult> OnGetAsync()
        {
            var now = DateTime.Now;

            // 1. Post Velocity
            Posts1h = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddHours(-1));
            Posts12h = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddHours(-12));
            Posts1d = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddDays(-1));
            Posts7d = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddDays(-7));

            // 2. Prepare Graph Data (Last 7 Days)
            var last7DaysDates = Enumerable.Range(0, 7).Select(i => now.Date.AddDays(-i)).Reverse().ToList();
            var labels = last7DaysDates.Select(d => d.ToString("MMM dd")).ToList();
            var data = new List<int>();

            // Fetch all posts from last 7 days to calculate groups in memory safely
            var recentPosts = await _context.Post
                .Where(p => p.CreatedAt >= now.AddDays(-7))
                .Select(p => p.CreatedAt.Date)
                .ToListAsync();

            foreach (var day in last7DaysDates)
            {
                data.Add(recentPosts.Count(d => d == day));
            }

            ChartLabelsJson = JsonSerializer.Serialize(labels);
            ChartDataJson = JsonSerializer.Serialize(data);

            // 3. Category Stats
            TotalLost = await _context.Post.CountAsync(p => p.Category == "Lost Pet");
            TotalFound = await _context.Post.CountAsync(p => p.Category == "Found Pet");
            TotalAdoptions = await _context.Post.CountAsync(p => p.Category == "Adoption");

            // 4. Recent Data
            RecentAccounts = await _context.Account
                .OrderByDescending(a => a.Id)
                .Take(5)
                .ToListAsync();

            RecentPets = await _context.Pet
                .Include(p => p.Account)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}