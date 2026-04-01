using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Posts
{
    // Upload limits MUST be at the class level
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public class CreateModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;
        private readonly IWebHostEnvironment _env;

        public CreateModel(PethubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verify if user is banned
            if (CurrentAccountId.HasValue)
            {
                var account = await _context.Account.FindAsync(CurrentAccountId.Value);
                if (account != null && account.AccountStatus == "Banned")
                {
                    TempData["Error"] = "Banned users cannot create posts.";
                    return RedirectToPage("/Landing");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            // Verify if user is banned before accepting submission
            if (CurrentAccountId.HasValue)
            {
                var account = await _context.Account.FindAsync(CurrentAccountId.Value);
                if (account != null && account.AccountStatus == "Banned")
                {
                    TempData["Error"] = "Banned users cannot create posts.";
                    return RedirectToPage("/Landing");
                }
            }

            // Remove server-controlled fields from model validation
            ModelState.Remove("Post.Account");
            ModelState.Remove("Post.Reports");
            ModelState.Remove("Post.Status");
            ModelState.Remove("Post.CreatedAt");
            ModelState.Remove("Post.ImagePath");
            ModelState.Remove("Post.AccountId");

            if (!ModelState.IsValid)
                return Page();

            // Handle optional image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                // Enforce 5 MB cap
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Image must be 5 MB or smaller.";
                    return Page();
                }

                // Restrict MIME types
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowed.Contains(imageFile.ContentType.ToLowerInvariant()))
                {
                    ErrorMessage = "Only JPG, PNG, GIF, and WebP images are accepted.";
                    return Page();
                }

                try
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploadsDir = Path.Combine(webRoot, "uploads");

                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    Post.ImagePath = $"/uploads/{fileName}";
                }
                catch
                {
                    ErrorMessage = "Image upload failed. Please try a different file.";
                    return Page();
                }
            }

            // Bind server-controlled data
            Post.AccountId = CurrentAccountId ?? 0;
            Post.Status = "Active";
            Post.CreatedAt = DateTime.Now;

            try
            {
                _context.Post.Add(Post);
                await _context.SaveChangesAsync();
            }
            catch
            {
                ErrorMessage = "Something went wrong saving your post. Please try again.";
                return Page();
            }

            TempData["Success"] = "Your post has been published!";
            return RedirectToPage("/Landing");
        }
    }
}