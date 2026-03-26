using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.AccountManagement
{
    public class DetailsModel : AdminPageModel
    {
        private readonly PethubContext _context;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(PethubContext context, ILogger<DetailsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Account Account { get; set; } = default!;

        // The password is always stored as a Base64 SHA256 hash — display it as-is.
        public string HashedPassword { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                _logger?.LogDebug("DetailsModel: OnGetAsync() started - Id={Id}", id);

                if (id == null)
                {
                    _logger?.LogWarning("DetailsModel: Id is null");
                    return NotFound();
                }

                var account = await _context.Account.FirstOrDefaultAsync(m => m.Id == id);

                if (account is null)
                {
                    _logger?.LogWarning("DetailsModel: Account not found - Id={Id}", id);
                    return NotFound();
                }

                Account = account;
                HashedPassword = Account.Password;
                _logger?.LogDebug("DetailsModel: Account loaded - Username={Username}", account.Username);

                return Page();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ DetailsModel: Exception in OnGetAsync");
                return NotFound();
            }
        }
    }
}
