using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.PetManagement
{
    // Changed from PageModel to AuthenticatedPageModel to fix the redirect
    public class DetailsModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public DetailsModel(PethubContext context)
        {
            _context = context;
        }

        public Pet Pet { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            // Find the pet and ensure it belongs to the logged-in user
            var pet = await _context.Pet.FirstOrDefaultAsync(m => m.Id == id);

            if (pet == null)
                return NotFound();

            // Security: If the pet doesn't belong to the current user, send them back
            if (pet.AccountId != (CurrentAccountId ?? 0))
                return RedirectToPage("./Index");

            Pet = pet;
            return Page();
        }
    }
}