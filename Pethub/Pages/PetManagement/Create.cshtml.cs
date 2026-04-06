using Microsoft.AspNetCore.Mvc;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.PetManagement
{
    // Apply size limits for the image upload
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

        public IActionResult OnGet() => Page();

        [BindProperty]
        public Pet Pet { get; set; } = default!;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            // Remove server-controlled fields from validation so it doesn't fail silently
            ModelState.Remove("Pet.Account");
            ModelState.Remove("Pet.AccountId");
            ModelState.Remove("Pet.ImagePath");

            if (!ModelState.IsValid)
                return Page();

            // Image Upload Logic 
            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Image must be 5 MB or smaller.";
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

                    Pet.ImagePath = $"/uploads/{fileName}";
                }
                catch (Exception ex)
                {
                    // Expose the exact system error to the UI to help debug permissions
                    ErrorMessage = $"Image upload failed: {ex.Message}";
                    return Page();
                }
            }

            // Tie the pet to the logged-in user
            Pet.AccountId = CurrentAccountId ?? 0;

            _context.Pet.Add(Pet);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}