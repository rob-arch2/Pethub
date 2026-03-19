using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

        // SHA256 hash of the account password for display
        public string HashedPassword { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FirstOrDefaultAsync(m => m.Id == id);

            if (account is not null)
            {
                Account = account;

                // if password already looks like a SHA256 hex string, show it as-is; otherwise compute SHA256
                var pwd = Account.Password ?? string.Empty;
                HashedPassword = Regex.IsMatch(pwd, "^[0-9a-fA-F]{64}$") ? pwd : ComputeSha256Hash(pwd);

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                Account = account;
                _context.Account.Remove(Account);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

        private static string ComputeSha256Hash(string raw)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha256.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
