using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pethub.Data; // <-- Added to access PethubContext

namespace Pethub.Models
{
    // Any page that needs a logged-in user should inherit this instead of PageModel.
    // It auto-redirects to Login if there's no active session — no need to repeat the check on every page.
    public abstract class AuthenticatedPageModel : PageModel
    {
        protected ILogger<AuthenticatedPageModel>? Logger { get; set; }

        // Publicly expose the current account id for use in page models and Razor views
        public int? CurrentAccountId
        {
            get
            {
                try
                {
                    if (HttpContext == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext is null");
                        return null;
                    }

                    if (HttpContext.Session == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext.Session is null");
                        return null;
                    }

                    var accountId = HttpContext.Session.GetInt32("AccountId");
                    Logger?.LogDebug("AuthenticatedPageModel: CurrentAccountId retrieved = {AccountId}", accountId);
                    return accountId;
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "AuthenticatedPageModel: Exception in CurrentAccountId getter");
                    return null;
                }
            }
        }

        // Backwards-compatible protected alias used across existing page models
        protected int? AccountId => CurrentAccountId;

        // Public username for display in views
        public string? AccountUsername
        {
            get
            {
                try
                {
                    if (HttpContext == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext is null when getting AccountUsername");
                        return null;
                    }

                    if (HttpContext.Session == null)
                    {
                        Logger?.LogWarning("AuthenticatedPageModel: HttpContext.Session is null when getting AccountUsername");
                        return null;
                    }

                    var username = HttpContext.Session.GetString("AccountUsername");
                    Logger?.LogDebug("AuthenticatedPageModel: AccountUsername retrieved = {Username}", username ?? "null");
                    return username;
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "AuthenticatedPageModel: Exception in AccountUsername getter");
                    return null;
                }
            }
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            try
            {
                // Try to get a logger from HttpContext for debugging
                var loggerFactory = HttpContext?.RequestServices?.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                var debugLogger = loggerFactory?.CreateLogger("AuthenticatedPageModel");
                debugLogger?.LogInformation("🔍 [AuthCheck-START] OnPageHandlerExecuting for page {PageName} handler {Handler} method {Method}",
                    context?.RouteData?.Values["page"] ?? "unknown",
                    context?.HandlerMethod?.Name ?? "unknown",
                    HttpContext?.Request?.Method ?? "unknown");

                Logger?.LogDebug("AuthenticatedPageModel: OnPageHandlerExecuting started for page {PageName}",
                    context!.RouteData.Values["page"]);

                // Guard: check if context is null
                if (context == null)
                {
                    Logger?.LogError("AuthenticatedPageModel: PageHandlerExecutingContext is null");
                    return;
                }

                // Guard: check if HttpContext exists
                if (HttpContext == null)
                {
                    Logger?.LogError("AuthenticatedPageModel: HttpContext is null, cannot authenticate user");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Guard: check if Session exists
                if (HttpContext.Session == null)
                {
                    Logger?.LogError("AuthenticatedPageModel: Session middleware not configured or disabled");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // Check if user is authenticated
                int? accountId = HttpContext.Session.GetInt32("AccountId");

                if (accountId == null || accountId == 0)
                {
                    Logger?.LogWarning("AuthenticatedPageModel: No valid session found, redirecting to Login");
                    context.Result = RedirectToPage("/Login");
                    return;
                }

                // ====================================================================
                // ── NEW: INSTANT BAN ENFORCER ───────────────────────────────────────
                // ====================================================================
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
                // ====================================================================

                Logger?.LogDebug("AuthenticatedPageModel: User authenticated with AccountId = {AccountId}", accountId);
                debugLogger?.LogInformation("🔍 [AuthCheck-PASS] Authentication successful, AccountId={AccountId}", accountId);
                base.OnPageHandlerExecuting(context);
                debugLogger?.LogInformation("[AuthCheck-END] OnPageHandlerExecuting completed successfully");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "❌ [AuthCheck-FAILED] Exception in OnPageHandlerExecuting: {ExceptionType}", ex.GetType().Name);
                Logger?.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                context.Result = RedirectToPage("/Login");
            }
        }
    }
}