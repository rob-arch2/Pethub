using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.UserDashboard
{
    public class IndexModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _env;

        public IndexModel(PethubContext context, ILogger<IndexModel> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        public Account Account { get; set; } = default!;
        public List<Post> UserPosts { get; set; } = new();
        public List<Pet> MyPets { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var id = CurrentAccountId ?? 0;
                var account = await _context.Account.FindAsync(id);
                if (account == null) return RedirectToPage("/Login");

                Account = account;

                UserPosts = await _context.Post
                    .Where(p => p.AccountId == id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

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

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            if (!CurrentAccountId.HasValue) return RedirectToPage("/Login");

            var post = await _context.Post.FirstOrDefaultAsync(p => p.Id == postId && p.AccountId == CurrentAccountId.Value);

            if (post != null)
            {
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

                _context.Post.Remove(post);

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