using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub.Models
{
    // Any page that needs a logged-in user should inherit this instead of PageModel.
    // It auto-redirects to Login if there's no active session — no need to repeat the check on every page.
    public abstract class AuthenticatedPageModel : PageModel
    {
        // Public properties available to Razor views
        public int? CurrentAccountId { get; private set; }
        public string? AccountUsername { get; private set; }

        // Protected alias for code-behind convenience
        protected int? AccountId => CurrentAccountId;

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            // No session = not logged in, send them back to Login
            var id = HttpContext.Session.GetInt32("AccountId");
            if (id == null)
            {
                context.Result = RedirectToPage("/Error");
                return;
            }

            // Populate public session-backed properties for views and code
            CurrentAccountId = id;
            AccountUsername = HttpContext.Session.GetString("AccountUsername");

            base.OnPageHandlerExecuting(context);
        }
    }
}