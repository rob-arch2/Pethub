using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Models;

namespace Pethub.Pages
{
    public class PrivacyModel : AuthenticatedPageModel
    {
        // Privacy page model - shows the site's privacy policy page to authenticated users
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
