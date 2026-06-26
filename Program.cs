using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext for PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Session services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15); // 15 minutes session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Initialize TimeZone globally
TimeZoneHelper.Initialize(app.Services.GetRequiredService<IConfiguration>());

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable session state (must be placed before UseAuthorization)
app.UseSession();

app.UseMiddleware<VisitorManagementSystem.Middleware.RememberMeMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
