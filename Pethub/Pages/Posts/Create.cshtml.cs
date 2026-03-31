using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Posts
{
    // The limits MUST be at the class level to prevent silent rejection of the image
    [RequestSizeLimit(10 * 1024 * 1024)]           // 10 MB — matches Kestrel cap
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

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            // These fields are set server-side or are navigation properties —
            // remove them from ModelState so they never block form validation.
            ModelState.Remove("Post.Account");
            ModelState.Remove("Post.Reports");
            ModelState.Remove("Post.Status");
            ModelState.Remove("Post.CreatedAt");
            ModelState.Remove("Post.ImagePath");
            ModelState.Remove("Post.AccountId");

            if (!ModelState.IsValid)
                return Page();

            // ── Optional image upload ────────────────────────────────────────
            if (imageFile != null && imageFile.Length > 0)
            {
                // Enforce 5 MB cap in code (Kestrel cap is 10 MB headroom)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Image must be 5 MB or smaller.";
                    return Page();
                }

                // Only allow common image MIME types
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowed.Contains(imageFile.ContentType.ToLowerInvariant()))
                {
                    ErrorMessage = "Only JPG, PNG, GIF, and WebP images are accepted.";
                    return Page();
                }

                try
                {
                    var webRoot = _env.WebRootPath
                                     ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
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

                    // Save the correct web path to the database (without wwwroot)
                    Post.ImagePath = $"/uploads/{fileName}";
                }
                catch
                {
                    ErrorMessage = "Image upload failed. Please try a different file.";
                    return Page();
                }
            }

            // ── Set all server-controlled fields ────────────────────────────
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