using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.PetManagement
{
    public class DetailsModel : PageModel
    {
        private readonly Pethub.Data.PethubContext _context;

        public DetailsModel(Pethub.Data.PethubContext context)
        {
            _context = context;
        }

        public Pet Pet { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pet.FirstOrDefaultAsync(m => m.Id == id);

            if (pet is not null)
            {
                Pet = pet;

                return Page();
            }

            return NotFound();
        }
    }
}
