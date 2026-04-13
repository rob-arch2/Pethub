using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.UserDashboard
{
    // Page for showing a user’s dashboard with their posts and pets
    public class IndexModel : AuthenticatedPageModel
    {
        // Database context for queries
        private readonly PethubContext _context;
        // Logger for tracking activity
        private readonly ILogger<IndexModel> _logger;
        // Environment for handling file paths
        private readonly IWebHostEnvironment _env;

        // Constructor sets up database, logger, and environment
        public IndexModel(PethubContext context, ILogger<IndexModel> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        // Current user account
        public Account Account { get; set; } = default!;
        // Posts created by the user
        public List<Post> UserPosts { get; set; } = new();
        // Pets owned by the user
        public List<Pet> MyPets { get; set; } = new();

        // Loads account, posts, and pets for the dashboard
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var id = CurrentAccountId ?? 0;
                var account = await _context.Account.FindAsync(id);
                if (account == null) return RedirectToPage("/Login");

                Account = account;

                // Get posts by the user
                UserPosts = await _context.Post
                    .Where(p => p.AccountId == id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Get pets owned by the user
                MyPets = await _context.Pet
                    .Where(p => p.AccountId == id)
                    .ToListAsync();

                return Page();
            }
            catch
            {
                return RedirectToPage("/Login");
            }
        }

        // Deletes a post owned by the user
        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            if (!CurrentAccountId.HasValue) return RedirectToPage("/Login");

            var post = await _context.Post.FirstOrDefaultAsync(p => p.Id == postId && p.AccountId == CurrentAccountId.Value);

            if (post != null)
            {
                // Delete image file if it exists
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var physicalPath = Path.Combine(webRoot, post.ImagePath.TrimStart('/'));
                    physicalPath = physicalPath.Replace('/', Path.DirectorySeparatorChar);

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                // Remove post from database
                _context.Post.Remove(post);

                // Log the delete action
                var log = new ActivityLog
                {
                    AccountId = CurrentAccountId.Value,
                    Role = "User",
                    Action = "Deleted Post",
                    Details = $"Permanently removed post titled '{post.Title}'",
                    Timestamp = DateTime.Now
                };
                _context.ActivityLog.Add(log);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Post deleted successfully.";
            }

            return RedirectToPage();
        }
    }
}
