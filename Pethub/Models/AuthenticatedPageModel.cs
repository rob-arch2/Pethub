using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub.Models
{
    // Any page that needs a logged-in user should inherit this instead of PageModel.
    // It auto-redirects to Login if there's no active session — no need to repeat the check on every page.
    public abstract class AuthenticatedPageModel : PageModel
    {
        protected int? AccountId => HttpContext.Session.GetInt32("AccountId");
        protected string? AccountUsername => HttpContext.Session.GetString("AccountUsername");

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            // No session = not logged in, send them back to Login
            if (HttpContext.Session.GetInt32("AccountId") == null)
            {
                context.Result = RedirectToPage("/Error");
                return;
            }
            base.OnPageHandlerExecuting(context);
        }
    }
}