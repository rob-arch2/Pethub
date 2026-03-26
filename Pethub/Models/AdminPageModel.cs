using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub.Models
{
    /// <summary>
    /// Base PageModel for admin-only pages.
    /// Redirects to /Login if the session role is not "Admin".
    /// </summary>
    public abstract class AdminPageModel : PageModel
    {
        protected ILogger<AdminPageModel>? Logger { get; set; }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            try
            {
                Logger?.LogDebug("AdminPageModel: OnPageHandlerExecuting started for page {PageName}", 
                    context.RouteData.Values["page"]);

                // Guard: check if context is null
                if (context == null)
                {
                    Logger?.LogError("AdminPageModel: PageHandlerExecutingContext is null");
                    return;
                }

                // Guard: check if HttpContext exists
                if (HttpContext == null)
                {
                    Logger?.LogError("AdminPageModel: HttpContext is null, cannot verify admin role");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Guard: check if Session exists
                if (HttpContext.Session == null)
                {
                    Logger?.LogError("AdminPageModel: Session middleware not configured or disabled");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Check if user has admin role
                string? accountRole = HttpContext.Session.GetString("AccountRole");
                Logger?.LogDebug("AdminPageModel: AccountRole from session = {Role}", accountRole ?? "null");

                if (string.IsNullOrEmpty(accountRole) || accountRole != "Admin")
                {
                    Logger?.LogWarning("AdminPageModel: User does not have admin role (role={Role}), denying access", 
                        accountRole ?? "null");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                Logger?.LogDebug("AdminPageModel: User verified as admin, granting access");
                base.OnPageHandlerExecuting(context);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "AdminPageModel: Exception in OnPageHandlerExecuting");
                context.Result = RedirectToPage("/Login");
            }
        }
    }
}