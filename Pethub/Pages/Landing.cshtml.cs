using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Models;

namespace Pethub.Pages
{
    public class LandingModel : AuthenticatedPageModel
    {
        // Landing page model - simple authenticated landing view after login
        public void OnGet()
        {
        }
    }
}
