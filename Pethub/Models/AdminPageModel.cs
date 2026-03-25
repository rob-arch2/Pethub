using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub.Models
{
    /// <summary>
    /// Base PageModel for admin-only pages.
    /// Redirects to /Error if the session role is not "Admin".
    /// </summary>
    public abstract class AdminPageModel : PageModel
    {
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (HttpContext.Session.GetString("AccountRole") != "Admin")
            {
                context.Result = RedirectToPage("/Error");
                return;
            }
            base.OnPageHandlerExecuting(context);
        }
    }
}