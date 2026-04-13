using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Posts
{
    // Page for editing an existing post
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public class EditModel : AuthenticatedPageModel
    {
        // Database context for queries
        private readonly PethubContext _context;
        // Environment for handling file uploads
        private readonly IWebHostEnvironment _env;

        // Constructor sets up database and environment
        public EditModel(PethubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Post object bound to the form
        [BindProperty]
        public Post Post { get; set; } = default!;

        // Holds error messages for display
        public string? ErrorMessage { get; set; }

        // Loads the post for editing if it belongs to the current user
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

        // Handles saving changes to the post
        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            if (!CurrentAccountId.HasValue)
                return RedirectToPage("/Login");

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

                ErrorMessage = "Please fill out all required fields: " + string.Join(" | ", hiddenErrors);
                return Page();
            }

            // Find the post to update
            var postToUpdate = await _context.Post.FirstOrDefaultAsync(p => p.Id == Post.Id && p.AccountId == CurrentAccountId.Value);
            if (postToUpdate == null)
                return RedirectToPage("/UserDashboard/Index");

            // Update post details
            postToUpdate.Title = Post.Title;
            postToUpdate.Description = Post.Description;
            postToUpdate.Category = Post.Category;
            postToUpdate.Price = Post.Price;
            postToUpdate.Location = Post.Location;

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
                    ErrorMessage = $"Format {imageFile.ContentType} is not supported.";
                    return Page();
                }

                try
                {
                    // Save new image and delete old one if it exists
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

            // Log the edit action
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
                // Save changes to database
                await _context.SaveChangesAsync();
                TempData["Success"] = "Your post has been updated!";
                return RedirectToPage("/UserDashboard/Index");
            }
            catch (Exception ex)
            {
                // Show error if saving fails
                ErrorMessage = $"Something went wrong saving your post: {ex.Message}";
                return Page();
            }
        }
    }
}
