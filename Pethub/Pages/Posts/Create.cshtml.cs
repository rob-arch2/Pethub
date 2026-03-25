using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Posts
{
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
            // Navigation property and server-set fields don't need to be in ModelState
            ModelState.Remove("Post.Account");
            ModelState.Remove("Post.Status");
            ModelState.Remove("Post.CreatedAt");
            ModelState.Remove("Post.ImagePath");

            if (!ModelState.IsValid)
                return Page();

            // ── Handle optional image upload ────────────────────────────
            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Image must be 5 MB or smaller.";
                    return Page();
                }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir); // creates folder if it doesn't exist

                var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                Post.ImagePath = $"/uploads/{fileName}";
            }

            // ── Set server-side fields ──────────────────────────────────
            Post.AccountId = AccountId ?? 0;
            Post.Status = "Active";
            Post.CreatedAt = DateTime.Now;

            _context.Post.Add(Post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your post has been published!";
            return RedirectToPage("/Landing");
        }
    }
}