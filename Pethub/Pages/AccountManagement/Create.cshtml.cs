using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.AccountManagement
{
    public class CreateModel : AdminPageModel
    {
        private readonly PethubContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(PethubContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            try
            {
                _logger?.LogDebug("CreateModel: OnGet() started");
                return Page();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ CreateModel: Exception in OnGet");
                return Page();
            }
        }

        [BindProperty]
        public Account Account { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger?.LogDebug("CreateModel: OnPostAsync() started - Username={Username}", Account?.Username);

                if (!ModelState.IsValid)
                {
                    _logger?.LogWarning("CreateModel: ModelState invalid");
                    return Page();
                }

                _logger?.LogDebug("CreateModel: Adding new account to database");
                _context.Account.Add(Account);
                await _context.SaveChangesAsync();

                _logger?.LogInformation("✓ CreateModel: Account created successfully - Username={Username}", Account.Username);
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ CreateModel: Exception during account creation");
                ModelState.AddModelError("", "An error occurred while creating the account.");
                return Page();
            }
        }
    }
}
