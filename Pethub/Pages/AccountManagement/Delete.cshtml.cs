using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.AccountManagement
{
    public class DeleteModel : AdminPageModel
    {
        private readonly PethubContext _context;

        public DeleteModel(PethubContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Account Account { get; set; } = default!;

        // The password is always stored as a Base64 SHA256 hash — display it as-is.
        public string HashedPassword { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var account = await _context.Account.FirstOrDefaultAsync(m => m.Id == id);

            if (account is null)
                return NotFound();

            Account = account;
            HashedPassword = Account.Password;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var account = await _context.Account.FindAsync(id);

            if (account != null)
            {
                Account = account;
                _context.Account.Remove(Account);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}