using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.PetManagement
{
    public class DeleteModel : AuthenticatedPageModel
    {
        private readonly PethubContext _context;

        public DeleteModel(PethubContext context)
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

            if (pet is not null)
            {
                Pet = pet;
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Use Pet.Id from [BindProperty] — same fix as account delete
            int id = Pet?.Id ?? 0;
            if (id == 0) return NotFound();

            var pet = await _context.Pet.FindAsync(id);
            if (pet != null)
            {
                // Log the pet deletion
                _context.ActivityLog.Add(new ActivityLog
                {
                    AccountId = CurrentAccountId ?? 0,
                    Role = "User",
                    Action = "Deleted Pet",
                    Details = $"Removed pet record: '{pet.Name}' ({pet.Species})",
                    Timestamp = DateTime.Now
                });

                Pet = pet;
                _context.Pet.Remove(Pet);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}