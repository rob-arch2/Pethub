using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub.Models
{
    /// <summary>
    /// Base PageModel for all pages that require an active login session.
    /// Inheriting this class automatically redirects unauthenticated users to /Login.
    /// </summary>
    public abstract class AuthenticatedPageModel : PageModel
    {
        protected int? AccountId => HttpContext.Session.GetInt32("AccountId");
        protected string? AccountUsername => HttpContext.Session.GetString("AccountUsername");

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (HttpContext.Session.GetInt32("AccountId") == null)
            {
                context.Result = RedirectToPage("/Error");
                return;
            }
            base.OnPageHandlerExecuting(context);
        }
    }
}