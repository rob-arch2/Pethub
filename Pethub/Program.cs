using Microsoft.EntityFrameworkCore;
using Pethub.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<PethubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PethubContext")
    ?? throw new InvalidOperationException("Connection string 'PethubContext' not found.")));

// Session
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

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession(); // must come before UseAuthorization
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapGet("/", () => Results.Redirect("/Login"));

app.Run();