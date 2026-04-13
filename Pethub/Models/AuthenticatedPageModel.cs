using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Data; // <-- Added to access PethubContext

namespace Pethub.Models
{
    // Base class for pages that require a logged-in user
    public abstract class AuthenticatedPageModel : PageModel
    {
        // Logger for tracking activity and errors
        protected ILogger<AuthenticatedPageModel>? Logger { get; set; }

        // Current account ID from session
        public int? CurrentAccountId
        {
            get
            {
                try
                {
                    // Return null if HttpContext is missing
                    if (HttpContext == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext is null");
                        return null;
                    }

                    // Return null if Session is missing
                    if (HttpContext.Session == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext.Session is null");
                        return null;
                    }

                    // Get account ID from session
                    var accountId = HttpContext.Session.GetInt32("AccountId");
                    Logger?.LogDebug("AuthenticatedPageModel: CurrentAccountId retrieved = {AccountId}", accountId);
                    return accountId;
                }
                catch (Exception ex)
                {
                    // Log error if something goes wrong
                    Logger?.LogError(ex, "AuthenticatedPageModel: Exception in CurrentAccountId getter");
                    return null;
                }
            }
        }

        // Shortcut alias for account ID
        protected int? AccountId => CurrentAccountId;

        // Current username from session
        public string? AccountUsername
        {
            get
            {
                try
                {
                    // Return null if HttpContext is missing
                    if (HttpContext == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext is null when getting AccountUsername");
                        return null;
                    }

                    // Return null if Session is missing
                    if (HttpContext.Session == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext.Session is null when getting AccountUsername");
                        return null;
                    }

                    // Get username from session
                    var username = HttpContext.Session.GetString("AccountUsername");
                    Logger?.LogDebug("AuthenticatedPageModel: AccountUsername retrieved = {Username}", username ?? "null");
                    return username;
                }
                catch (Exception ex)
                {
                    // Log error if something goes wrong
                    Logger?.LogError(ex, "AuthenticatedPageModel: Exception in AccountUsername getter");
                    return null;
                }
            }
        }

        // Runs before the page handler executes
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            try
            {
                // Create a temporary logger for debugging
                var loggerFactory = HttpContext?.RequestServices?.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                var debugLogger = loggerFactory?.CreateLogger("AuthenticatedPageModel");
                debugLogger?.LogInformation("🔍 [AuthCheck-START] OnPageHandlerExecuting for page {PageName} handler {Handler} method {Method}",
                    context?.RouteData?.Values["page"] ?? "unknown",
                    context?.HandlerMethod?.Name ?? "unknown",
                    HttpContext?.Request?.Method ?? "unknown");

                // Log that the check has started
                Logger?.LogDebug("AuthenticatedPageModel: OnPageHandlerExecuting started for page {PageName}",
                    context!.RouteData.Values["page"]);

                // Stop if context is missing
                if (context == null)
                {
                    Logger?.LogError("AuthenticatedPageModel: PageHandlerExecutingContext is null");
                    return;
                }

                // Stop if HttpContext is missing
                if (HttpContext == null)
                {
                    Logger?.LogError("AuthenticatedPageModel: HttpContext is null, cannot authenticate user");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Stop if Session is missing
                if (HttpContext.Session == null)
                {
                    Logger?.LogError("AuthenticatedPageModel: Session middleware not configured or disabled");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Get account ID from session
                int? accountId = HttpContext.Session.GetInt32("AccountId");

                // Redirect if no valid account ID
                if (accountId == null || accountId == 0)
                {
                    Logger?.LogWarning("AuthenticatedPageModel: No valid session found, redirecting to Login");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Check if user is banned
                var dbContext = HttpContext.RequestServices.GetService(typeof(PethubContext)) as PethubContext;
                if (dbContext != null)
                {
                    var user = dbContext.Account.Find(accountId.Value);
                    if (user == null || user.AccountStatus == "Banned")
                    {
                        debugLogger?.LogWarning("🚫 [AuthCheck-BANNED] User {AccountId} is banned or deleted. Destroying session.", accountId);
                        HttpContext.Session.Clear();
                        context.Result = RedirectToPage("/Login");
                        return;
                    }
                }

                // Allow access if authenticated and not banned
                Logger?.LogDebug("AuthenticatedPageModel: User authenticated with AccountId = {AccountId}", accountId);
                debugLogger?.LogInformation("🔍 [AuthCheck-PASS] Authentication successful, AccountId={AccountId}", accountId);
                base.OnPageHandlerExecuting(context);
                debugLogger?.LogInformation("[AuthCheck-END] OnPageHandlerExecuting completed successfully");
            }
            catch (Exception ex)
            {
                // Redirect to login if an error happens
                Logger?.LogError(ex, "❌ [AuthCheck-FAILED] Exception in OnPageHandlerExecuting: {ExceptionType}", ex.GetType().Name);
                Logger?.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                context.Result = RedirectToPage("/Login");
            }
        }
    }
}
