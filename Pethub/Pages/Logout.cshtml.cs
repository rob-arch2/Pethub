using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pethub.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        // Clear the entire session then send the user back to Login
        public IActionResult OnGet()
        {
            try
            {
                _logger?.LogDebug("LogoutModel: OnGet() started - user logout request");

                // Guard: check if HttpContext exists
                if (HttpContext == null)
                {
                    _logger?.LogError("LogoutModel: HttpContext is null, cannot clear session");
                    return RedirectToPage("/Login");
                }

                // Guard: check if Session exists
                if (HttpContext.Session == null)
                {
                    _logger?.LogWarning("LogoutModel: Session is null, nothing to clear");
                    return RedirectToPage("/Login");
                }

                HttpContext.Session.Clear();
                _logger?.LogInformation("✓ User logout successful - session cleared");

                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "  LogoutModel: Exception during session clear");
                // Even if session clear fails, redirect to login
                return RedirectToPage("/Login");
            }
        }
    }
}