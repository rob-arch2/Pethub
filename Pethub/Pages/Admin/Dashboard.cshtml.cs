using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;
using System.Text.Json;

namespace Pethub.Pages.Admin
{
    // Dashboard page for admin with stats and recent data
    public class DashboardModel : AdminPageModel
    {
        // Database context for queries
        private readonly PethubContext _context;
        // Logger for tracking activity
        private readonly ILogger<DashboardModel> _logger;

        // Constructor sets up database and logger
        public DashboardModel(PethubContext context, ILogger<DashboardModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Total accounts and gender counts
        public int TotalAccounts { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }

        // Age group counts
        public int AgeGroup1 { get; set; }
        public int AgeGroup2 { get; set; }
        public int AgeGroup3 { get; set; }
        public int AgeGroup4 { get; set; }

        // Percentages for gender and age groups
        public double MalePercent { get; set; }
        public double FemalePercent { get; set; }
        public double Ag1p { get; set; }
        public double Ag2p { get; set; }
        public double Ag3p { get; set; }
        public double Ag4p { get; set; }

        // Post counts over different time ranges
        public int Posts1h { get; set; }
        public int Posts12h { get; set; }
        public int Posts1d { get; set; }
        public int Posts7d { get; set; }

        // Chart data for posts in the last 7 days
        public string ChartLabelsJson { get; set; } = "[]";
        public string ChartDataJson { get; set; } = "[]";

        // Category totals for posts
        public int TotalLost { get; set; }
        public int TotalFound { get; set; }
        public int TotalAdoptions { get; set; }

        // Recent accounts and pets for display
        public IList<Account> RecentAccounts { get; set; } = new List<Account>();
        public IList<Pet> RecentPets { get; set; } = new List<Pet>();

        // Loads dashboard data when the page is requested
        public async Task OnGetAsync()
        {
            try
            {
                _logger?.LogDebug("DashboardModel: OnGetAsync() started");

                // Get all accounts and calculate totals
                var accounts = await _context.Account.ToListAsync();
                TotalAccounts = accounts.Count;
                MaleCount = accounts.Count(a => a.Gender == "Male");
                FemaleCount = accounts.Count(a => a.Gender == "Female");

                // Count accounts by age groups
                AgeGroup1 = accounts.Count(a => a.Age >= 18 && a.Age <= 25);
                AgeGroup2 = accounts.Count(a => a.Age >= 26 && a.Age <= 35);
                AgeGroup3 = accounts.Count(a => a.Age >= 36 && a.Age <= 50);
                AgeGroup4 = accounts.Count(a => a.Age >= 51);

                // Calculate percentages if accounts exist
                if (TotalAccounts > 0)
                {
                    MalePercent = Math.Round((double)MaleCount / TotalAccounts * 100, 1);
                    FemalePercent = Math.Round((double)FemaleCount / TotalAccounts * 100, 1);
                    Ag1p = Math.Round((double)AgeGroup1 / TotalAccounts * 100, 1);
                    Ag2p = Math.Round((double)AgeGroup2 / TotalAccounts * 100, 1);
                    Ag3p = Math.Round((double)AgeGroup3 / TotalAccounts * 100, 1);
                    Ag4p = Math.Round((double)AgeGroup4 / TotalAccounts * 100, 1);
                }

                // Count posts created in different time ranges
                var now = DateTime.Now;
                Posts1h = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddHours(-1));
                Posts12h = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddHours(-12));
                Posts1d = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddDays(-1));
                Posts7d = await _context.Post.CountAsync(p => p.CreatedAt >= now.AddDays(-7));

                // Prepare chart data for posts in the last 7 days
                var last7DaysDates = Enumerable.Range(0, 7).Select(i => now.Date.AddDays(-i)).Reverse().ToList();
                var labels = last7DaysDates.Select(d => d.ToString("MMM dd")).ToList();
                var data = new List<int>();

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

                // Count posts by category
                TotalLost = await _context.Post.CountAsync(p => p.Category == "Lost Pet");
                TotalFound = await _context.Post.CountAsync(p => p.Category == "Found Pet");
                TotalAdoptions = await _context.Post.CountAsync(p => p.Category == "Adoption");

                // Get recent accounts and pets
                RecentAccounts = accounts.OrderByDescending(a => a.Id).Take(5).ToList();
                RecentPets = await _context.Pet
                    .Include(p => p.Account)
                    .OrderByDescending(p => p.Id)
                    .Take(5)
                    .ToListAsync();

                _logger?.LogDebug("✓ DashboardModel: Dashboard loaded successfully. Total Accounts={Total}, Recent Pets={Pets}", TotalAccounts, RecentPets.Count);
            }
            catch (Exception ex)
            {
                // Log error and reset totals if something fails
                _logger?.LogError(ex, "❌ DashboardModel: Exception in OnGetAsync");
                TotalAccounts = 0;
            }
        }
    }
}
