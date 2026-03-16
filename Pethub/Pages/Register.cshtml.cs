using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string FullName { get; set; }

        [BindProperty]
        public string ContactNumber { get; set; }

        [BindProperty]
        public string Gender { get; set; }

        [BindProperty]
        public DateTime BirthDate { get; set; }

        [BindProperty]
        public int Age { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }
        public void OnGet()
        {
        }
    }
}
