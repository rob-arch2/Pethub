using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.PetManagement
{
    public class EditModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public EditModel(PethubContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Pet Pet { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var pet = await _context.Pet.FirstOrDefaultAsync(m => m.Id == id);
            if (pet == null)
                return NotFound();

            // Only allow the owner to edit their own pet
            if (pet.AccountId != (CurrentAccountId ?? 0))
                return RedirectToPage("./Index");

            Pet = pet;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove validation for related data we don't handle in the form
            ModelState.Remove("Pet.Account");
            ModelState.Remove("Pet.AccountId");
            ModelState.Remove("Pet.ImagePath");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ensure the pet still belongs to the current user
            var existingPet = await _context.Pet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Pet.Id);
            if (existingPet == null || existingPet.AccountId != (CurrentAccountId ?? 0))
            {
                return RedirectToPage("./Index");
            }

            // Keep the original AccountId and ImagePath
            Pet.AccountId = existingPet.AccountId;
            if (string.IsNullOrEmpty(Pet.ImagePath))
            {
                Pet.ImagePath = existingPet.ImagePath;
            }

            _context.Attach(Pet).State = EntityState.Modified;

            try
            {
                // Log the pet edit
                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = CurrentAccountId ?? 0,
                    Role = "User",
                    Action = "Edited Pet",
                    Details = $"Updated details for pet '{Pet.Name}'",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pet.Any(e => e.Id == Pet.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("./Index");
        }
    }
}