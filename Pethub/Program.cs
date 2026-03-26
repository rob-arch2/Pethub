using Microsoft.EntityFrameworkCore;
using Pethub.Data;

// ⚠️ CATCH ALL UNHANDLED EXCEPTIONS AT APPDOMAIN LEVEL
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    var ex = e.ExceptionObject as Exception;
    var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("AppDomain");
    logger.LogCritical(ex, "🔴 [CRITICAL] Unhandled exception at AppDomain level: {ExceptionType}", ex?.GetType().FullName);
    if (ex?.InnerException != null)
    {
        logger.LogCritical("Inner exception: {InnerType}: {InnerMessage}", 
            ex.InnerException.GetType().FullName, ex.InnerException.Message);
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add logging - this is critical for debugging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Debug); // Show all debug messages
});

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Application starting up...");

builder.Services.AddRazorPages();

// Database
try
{
    var connectionString = builder.Configuration.GetConnectionString("PethubContext");

    if (string.IsNullOrEmpty(connectionString))
    {
        logger.LogError("❌ FATAL: Connection string 'PethubContext' not found in configuration");
        throw new InvalidOperationException("Connection string 'PethubContext' not found.");
    }

    logger.LogInformation("✓ Database connection string found");

    builder.Services.AddDbContext<PethubContext>(options =>
    {
        logger.LogDebug("Configuring DbContext with SQL Server...");
        options.UseSqlServer(connectionString);
    });

    logger.LogInformation("✓ DbContext registered successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Error configuring database context");
    throw;
}

// Session - CRITICAL for authentication
try
{
    logger.LogDebug("Configuring session services...");
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
    logger.LogInformation("✓ Session configured successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Error configuring session");
    throw;
}

var app = builder.Build();

logger.LogInformation("Building application pipeline...");

if (!app.Environment.IsDevelopment())
{
    logger.LogInformation("Environment: Production - Using exception handler");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    logger.LogInformation("Environment: Development - Detailed error pages enabled");
}

// ⚠️ GLOBAL EXCEPTION MIDDLEWARE - catches any unhandled exceptions
app.Use(async (context, next) =>
{
    try
    {
        if (context.Request.Method == "POST")
        {
            logger.LogInformation("📤 POST REQUEST: {Path} from {RemoteIP}",
                context.Request.Path, context.Connection.RemoteIpAddress);
            logger.LogInformation("📤 Content-Type: {ContentType}", context.Request.ContentType);
        }

        await next();

        if (context.Request.Method == "POST")
        {
            logger.LogInformation("✅ POST REQUEST COMPLETED: {Path} with status {StatusCode}",
                context.Request.Path, context.Response.StatusCode);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ CRITICAL EXCEPTION in middleware pipeline");
        logger.LogError("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        logger.LogError("Exception Type: {ExceptionType}", ex.GetType().FullName);
        logger.LogError("Exception Message: {Message}", ex.Message);
        logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);

        if (ex.InnerException != null)
        {
            logger.LogError("Inner Exception: {InnerExceptionType}: {InnerMessage}",
                ex.InnerException.GetType().FullName, ex.InnerException.Message);
        }

        // Try to return error response if response hasn't started
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"Internal Server Error: {ex.GetType().Name}: {ex.Message}");
        }
        throw;
    }
});

// Middleware pipeline order is CRITICAL
logger.LogDebug("Setting up middleware pipeline...");

app.UseHttpsRedirection();
logger.LogDebug("✓ HTTPS redirection middleware added");

app.UseRouting();
logger.LogDebug("✓ Routing middleware added");

// Session MUST come before authorization
app.UseSession();
logger.LogDebug("✓ Session middleware added");

app.UseAuthorization();
logger.LogDebug("✓ Authorization middleware added");

app.MapStaticAssets();
logger.LogDebug("✓ Static assets mapped");

app.MapRazorPages().WithStaticAssets();
logger.LogDebug("✓ Razor Pages mapped");

app.MapGet("/", () => Results.Redirect("/Login"));
logger.LogDebug("✓ Root redirect mapped to /Login");

logger.LogInformation("✅ Application pipeline configured. Starting to listen...");

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ FATAL: Application crashed");
    throw;
}