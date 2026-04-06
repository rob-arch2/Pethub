using Microsoft.AspNetCore.Mvc;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.PetManagement
{
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
            ModelState.Remove("Pet.Account");
            ModelState.Remove("Pet.AccountId");
            ModelState.Remove("Pet.ImagePath");

            if (!ModelState.IsValid)
                return Page();

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
                        Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    Pet.ImagePath = $"/uploads/{fileName}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Image upload failed: {ex.Message}";
                    return Page();
                }
            }

            Pet.AccountId = CurrentAccountId ?? 0;

            _context.Pet.Add(Pet);

            // Log the pet creation
            _context.ActivityLog.Add(new ActivityLog
            {
                AccountId = CurrentAccountId ?? 0,
                Role = "User",
                Action = "Added Pet",
                Details = $"Registered a new pet: '{Pet.Name}' ({Pet.Species})",
                Timestamp = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}