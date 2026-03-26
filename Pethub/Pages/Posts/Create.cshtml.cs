using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.Posts
{
    public class CreateModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(PethubContext context, IWebHostEnvironment env, ILogger<CreateModel> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _logger?.LogInformation("🔧 CreateModel constructor called - dependencies injected successfully");
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                _logger?.LogInformation("🎯 FIRST LINE: CreateModel.OnGet() STARTING execution");

                // CRITICAL: Initialize Post object with all required properties set
                // The null! declarations only silence compiler warnings; properties are actually null at runtime
                if (Post == null)
                {
                    _logger?.LogInformation("🎯 Initializing Post object (was null)");
                    Post = new Post
                    {
                        Title = string.Empty,
                        Description = string.Empty,
                        Location = string.Empty,
                        Category = string.Empty,
                        Status = "Active",
                        CreatedAt = DateTime.Now,
                        Reports = new List<Report>()
                    };
                    _logger?.LogInformation("🎯 Post object fully initialized with all string properties set to empty strings");
                }

                _logger?.LogInformation("🎯 Post object is: {PostState}", Post == null ? "NULL" : $"initialized with Id={Post.Id}, Title='{Post.Title}'");
                _logger?.LogInformation("🎯 About to return Page()");
                var result = Page();
                _logger?.LogInformation("🎯 Page() returned successfully: {ResultType}", result?.GetType().Name ?? "null");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🚨 EXCEPTION IN OnGet: {ExceptionType}: {Message}", ex.GetType().Name, ex.Message);
                _logger?.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            _logger?.LogInformation("═══════════════════════════════════════════════════════");
            _logger?.LogInformation("🚨 OnPostAsync ENTRY POINT - FIRST EXECUTABLE LINE");
            _logger?.LogInformation("ImageFile: {HasFile}", imageFile != null ? $"YES ({imageFile.FileName})" : "NO");
            _logger?.LogInformation("Post object: {PostState}", Post == null ? "NULL" : "NOT NULL");
            _logger?.LogInformation("═══════════════════════════════════════════════════════");

            try
            {
                // CRITICAL: Ensure Post object exists before any processing
                if (Post == null)
                {
                    _logger?.LogInformation("🎯 WARNING: Post object is NULL at OnPostAsync entry - initializing");
                    Post = new Post
                    {
                        Title = string.Empty,
                        Description = string.Empty,
                        Location = string.Empty,
                        Category = string.Empty,
                        Status = "Active",
                        CreatedAt = DateTime.Now,
                        Reports = new List<Report>()
                    };
                    _logger?.LogInformation("✅ Post object initialized");
                }

                _logger?.LogInformation("🎯 OnPostAsync execution started - ImageFile={HasFile}, Title={Title}", 
                    imageFile != null ? "YES" : "NO", Post?.Title ?? "null");

                // Remove all server-set and navigation properties from ModelState
                // so they don't block validation — these are never in the form POST
                ModelState.Remove("Post.Account");
                ModelState.Remove("Post.Reports");
                ModelState.Remove("Post.Status");
                ModelState.Remove("Post.CreatedAt");
                ModelState.Remove("Post.ImagePath");
                ModelState.Remove("Post.AccountId");

                if (!ModelState.IsValid)
                {
                    _logger?.LogWarning("CreateModel: ModelState is invalid");
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                    _logger?.LogWarning("CreateModel: Validation errors: {Errors}", errors);
                    return Page();
                }

                // ── Handle optional image upload ────────────────────────────
                if (imageFile != null && imageFile.Length > 0)
                {
                    _logger?.LogDebug("CreateModel: Image file provided - FileName={FileName}, Length={Length}",
                        imageFile.FileName, imageFile.Length);

                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        _logger?.LogWarning("CreateModel: Image file too large - {Length} bytes", imageFile.Length);
                        ErrorMessage = "Image must be 5 MB or smaller.";
                        return Page();
                    }

                    try
                    {
                        var webRoot = _env.WebRootPath
                            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                        var uploadsDir = Path.Combine(webRoot, "uploads");
                        Directory.CreateDirectory(uploadsDir);
                        _logger?.LogDebug("CreateModel: Uploads directory created/verified - {UploadDir}", uploadsDir);

                        var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadsDir, fileName);

                        await using var stream = new FileStream(filePath, FileMode.Create);
                        await imageFile.CopyToAsync(stream);

                        Post.ImagePath = $"/uploads/{fileName}";
                        _logger?.LogInformation("✓ Image uploaded successfully - {FileName}", fileName);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "❌ Image upload failed");
                        ErrorMessage = "Image upload failed. Please try a different file.";
                        return Page();
                    }
                }

                // ── Validate and set AccountId ──────────────────────────────
                if (AccountId == null || AccountId <= 0)
                {
                    _logger?.LogWarning("CreateModel: Invalid AccountId - AccountId={AccountId}", AccountId);
                    ErrorMessage = "Your session has expired. Please log in again.";
                    return RedirectToPage("/Login");
                }

                // ── Set all server-side fields explicitly ───────────────────
                Post.AccountId = AccountId.Value;
                Post.Status = "Active";
                Post.CreatedAt = DateTime.Now;

                _logger?.LogDebug("CreateModel: Post properties set - AccountId={AccountId}, Status={Status}, CreatedAt={CreatedAt}",
                    Post.AccountId, Post.Status, Post.CreatedAt);

                try
                {
                    _logger?.LogDebug("CreateModel: Adding post to database...");
                    _context.Post.Add(Post);

                    _logger?.LogDebug("CreateModel: Saving changes to database...");
                    await _context.SaveChangesAsync();

                    _logger?.LogInformation("✓ Post created successfully - PostId={PostId}, Title={Title}",
                        Post.Id, Post.Title);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger?.LogError(dbEx, "❌ Database update failed - PostId={PostId}", Post.Id);
                    ErrorMessage = "Database error while saving. Please try again.";
                    return Page();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "❌ Failed to save post to database");
                    ErrorMessage = "Something went wrong saving your post. Please try again.";
                    return Page();
                }

                // ── Redirect after successful post creation ──────────────────
                _logger?.LogInformation("🔄 Setting TempData[\"Success\"] before redirect...");
                TempData["Success"] = "Your post has been published!";
                _logger?.LogInformation("🔄 TempData set successfully. Redirecting to /Landing...");

                return RedirectToPage("/Landing");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Unhandled exception in CreateModel.OnPostAsync()");
                ErrorMessage = "An unexpected error occurred. Please try again.";
                return Page();
            }
        }
    }
}