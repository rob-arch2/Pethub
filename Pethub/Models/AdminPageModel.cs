using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub.Models
{
    // Base class for admin-only pages
    public abstract class AdminPageModel : PageModel
    {
        // Logger for tracking messages and errors
        protected ILogger<AdminPageModel>? Logger { get; set; }

        // Runs before the page handler executes
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            try
            {
                // Log that the check has started
                Logger?.LogDebug("AdminPageModel: OnPageHandlerExecuting started for page {PageName}",
                    context.RouteData.Values["page"]);

                // Stop if context is missing
                if (context == null)
                {
                    Logger?.LogError("AdminPageModel: PageHandlerExecutingContext is null");
                    return;
                }

                // Stop if HttpContext is missing
                if (HttpContext == null)
                {
                    Logger?.LogError("AdminPageModel: HttpContext is null, cannot verify admin role");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Stop if Session is missing
                if (HttpContext.Session == null)
                {
                    Logger?.LogError("AdminPageModel: Session middleware not configured or disabled");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Get the user’s role from session
                string? accountRole = HttpContext.Session.GetString("AccountRole");
                Logger?.LogDebug("AdminPageModel: AccountRole from session = {Role}", accountRole ?? "null");

                // Redirect if role is not Admin
                if (string.IsNullOrEmpty(accountRole) || accountRole != "Admin")
                {
                    Logger?.LogWarning("AdminPageModel: User does not have admin role (role={Role}), denying access",
                        accountRole ?? "null");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Allow access if user is Admin
                Logger?.LogDebug("AdminPageModel: User verified as admin, granting access");
                base.OnPageHandlerExecuting(context);
            }
            catch (Exception ex)
            {
                // Redirect to login if an error happens
                Logger?.LogError(ex, "AdminPageModel: Exception in OnPageHandlerExecuting");
                context.Result = RedirectToPage("/Login");
            }
        }
    }
}
