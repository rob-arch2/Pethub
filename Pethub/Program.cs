using Microsoft.EntityFrameworkCore;
using Pethub.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Kestrel: enough headroom for multipart image uploads ─────────────────────
// Page handler enforces the 5 MB user-facing cap; Kestrel needs to be
// at least as large as the cap plus multipart form overhead.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

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
// MapStaticAssets() only knows about files in its compile-time manifest
// and will 404 on dynamically created files — it must NOT replace UseStaticFiles().
app.UseStaticFiles();

app.UseRouting();
app.UseSession();       // must be before UseAuthorization
app.UseAuthorization();

// Keep MapStaticAssets for fingerprinted CSS/JS/lib assets from the build
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapGet("/", () => Results.Redirect("/Login"));

app.Run();