using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Kestrel: enough headroom for multipart image uploads ─────────────────────
// ── Kestrel: enough headroom for multipart image uploads ─────────────────────
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB

    // NEW: Stop the server from crashing if Brave sends bloated localhost cookies
    options.Limits.MaxRequestHeadersTotalSize = 131072; // 128 KB (Default is 32 KB)
});
// ==============================================================================
// ── CRITICAL FIX: GLOBAL FORM OPTIONS ─────────────────────────────────────────
// ==============================================================================
// 1. Prevents Anti-Forgery token validation from crashing the connection.
// 2. MemoryBufferThreshold forces files under 10MB to stay in RAM during upload, 
//    preventing the server from crashing due to Windows %TEMP% folder write permissions.
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
    options.ValueLengthLimit = 10 * 1024 * 1024;         // 10 MB
    options.MemoryBufferThreshold = 10 * 1024 * 1024;    // Keep up to 10MB in RAM!
});
// ==============================================================================

builder.Services.AddRazorPages();

builder.Services.AddDbContext<PethubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PethubContext")
    ?? throw new InvalidOperationException("Connection string 'PethubContext' not found.")));

// ── Session ──────────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// ── Middleware pipeline — order is critical ───────────────────────────────────
app.UseHttpsRedirection();

// UseStaticFiles() serves ANY file present in wwwroot/ at request time,
// including images uploaded at runtime to wwwroot/uploads/.
app.UseStaticFiles();

app.UseRouting();
app.UseSession();       // must be before UseAuthorization
app.UseAuthorization();

// Keep MapStaticAssets for fingerprinted CSS/JS/lib assets from the build
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapGet("/", () => Results.Redirect("/Login"));

app.Run();