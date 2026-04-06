using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Posts
{
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public class EditModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;
        private readonly IWebHostEnvironment _env;

        public EditModel(PethubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || !CurrentAccountId.HasValue)
                return RedirectToPage("/Login");

            var post = await _context.Post.FirstOrDefaultAsync(m => m.Id == id && m.AccountId == CurrentAccountId.Value);

            if (post == null)
                return RedirectToPage("/UserDashboard/Index");

            Post = post;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            if (!CurrentAccountId.HasValue)
                return RedirectToPage("/Login");

            ModelState.Remove("Post.Account");
            ModelState.Remove("Post.Reports");
            ModelState.Remove("Post.Status");
            ModelState.Remove("Post.CreatedAt");
            ModelState.Remove("Post.ImagePath");
            ModelState.Remove("Post.AccountId");

            if (!ModelState.IsValid)
            {
                // Pull exactly what failed so it doesn't fail silently
                var hiddenErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                    .Where(msg => !string.IsNullOrEmpty(msg));

                ErrorMessage = "Please fill out all required fields: " + string.Join(" | ", hiddenErrors);
                return Page();
            }

            var postToUpdate = await _context.Post.FirstOrDefaultAsync(p => p.Id == Post.Id && p.AccountId == CurrentAccountId.Value);
            if (postToUpdate == null)
                return RedirectToPage("/UserDashboard/Index");

            postToUpdate.Title = Post.Title;
            postToUpdate.Description = Post.Description;
            postToUpdate.Category = Post.Category;
            postToUpdate.Price = Post.Price;
            postToUpdate.Location = Post.Location; // NEW: Save the location!

            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Image must be 5 MB or smaller.";
                    return Page();
                }

                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/heic", "image/heif" };
                if (!allowed.Contains(imageFile.ContentType.ToLowerInvariant()))
                {
                    ErrorMessage = $"Format {imageFile.ContentType} is not supported.";
                    return Page();
                }

                try
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploadsDir = Path.Combine(webRoot, "uploads");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    if (!string.IsNullOrEmpty(postToUpdate.ImagePath))
                    {
                        var oldPath = Path.Combine(webRoot, postToUpdate.ImagePath.TrimStart('/'));
                        oldPath = oldPath.Replace('/', Path.DirectorySeparatorChar);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    postToUpdate.ImagePath = $"/uploads/{fileName}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Image upload failed: {ex.Message}";
                    return Page();
                }
            }

            var log = new ActivityLog
            {
                AccountId = CurrentAccountId.Value,
                Role = "User",
                Action = "Edited Post",
                Details = $"Updated the details for '{postToUpdate.Title}'",
                Timestamp = DateTime.Now
            };
            _context.ActivityLog.Add(log);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Your post has been updated!";
                return RedirectToPage("/UserDashboard/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Something went wrong saving your post: {ex.Message}";
                return Page();
            }
        }
    }
}