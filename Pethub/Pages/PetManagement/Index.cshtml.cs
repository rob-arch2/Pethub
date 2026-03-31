using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.PetManagement
{
    // Inherit from AuthenticatedPageModel so we can filter by CurrentAccountId
    public class IndexModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public IndexModel(PethubContext context)
        {
            _context = context;
        }

        public IList<Pet> Pet { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Pet != null)
            {
                // Only load the pets that belong to the logged-in user
                Pet = await _context.Pet
                    .Where(p => p.AccountId == CurrentAccountId)
                    .ToListAsync();
            }
        }
    }
}