using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Posts
{
    // Page for creating a new post
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public class CreateModel : AuthenticatedPageModel
    {
        // Database context for queries
        private readonly PethubContext _context;
        // Environment for handling file uploads
        private readonly IWebHostEnvironment _env;

        // Constructor sets up database and environment
        public CreateModel(PethubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Post object bound to the form
        [BindProperty]
        public Post Post { get; set; } = default!;

        // Holds error messages for display
        public string? ErrorMessage { get; set; }

        // Checks if banned users are blocked from creating posts
        public async Task<IActionResult> OnGetAsync()
        {
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

        // Handles post creation and image upload
        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            // Block banned users from posting
            if (CurrentAccountId.HasValue)
            {
                var account = await _context.Account.FindAsync(CurrentAccountId.Value);
                if (account != null && account.AccountStatus == "Banned")
                {
                    TempData["Error"] = "Banned users cannot create posts.";
                    return RedirectToPage("/Landing");
                }
            }

            // Remove auto-managed fields from validation
            ModelState.Remove("Post.Account");
            ModelState.Remove("Post.Reports");
            ModelState.Remove("Post.Status");
            ModelState.Remove("Post.CreatedAt");
            ModelState.Remove("Post.ImagePath");
            ModelState.Remove("Post.AccountId");

            // Show errors if validation fails
            if (!ModelState.IsValid)
            {
                var hiddenErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                    .Where(msg => !string.IsNullOrEmpty(msg));

                ErrorMessage = "Validation Blocked the Save: " + string.Join(" | ", hiddenErrors);
                return Page();
            }

            // Handle image upload if provided
            if (imageFile != null && imageFile.Length > 0)
            {
                // Block files larger than 5 MB
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Image must be 5 MB or smaller.";
                    return Page();
                }

                // Allow only specific image formats
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/heic", "image/heif" };
                if (!allowed.Contains(imageFile.ContentType.ToLowerInvariant()))
                {
                    ErrorMessage = $"Format {imageFile.ContentType} is not supported. Use JPG, PNG, GIF, WebP, or HEIC.";
                    return Page();
                }

                try
                {
                    // Save image to uploads folder
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
                catch (Exception ex)
                {
                    ErrorMessage = $"Image upload failed: {ex.Message}";
                    return Page();
                }
            }

            // Set post details before saving
            Post.AccountId = CurrentAccountId ?? 0;
            Post.Status = "Active";
            Post.CreatedAt = DateTime.Now;

            try
            {
                // Save post to database
                _context.Post.Add(Post);

                // Log the post creation
                var log = new ActivityLog
                {
                    AccountId = CurrentAccountId.Value,
                    Role = "User",
                    Action = "Published Post",
                    Details = $"Created a {Post.Category} post titled '{Post.Title}'",
                    Timestamp = DateTime.Now
                };
                _context.ActivityLog.Add(log);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Show error if saving fails
                ErrorMessage = $"Something went wrong saving your post: {ex.Message}";
                return Page();
            }

            // Show success message and redirect
            TempData["Success"] = "Your post has been published!";
            return RedirectToPage("/Landing");
        }
    }
}
