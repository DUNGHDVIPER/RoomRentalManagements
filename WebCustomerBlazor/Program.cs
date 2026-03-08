using DAL.Data;
using DAL.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebCustomerBlazor.Components;
using BLL.Services;
using BLL.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// =====================
// 1) DB + Identity
// =====================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie Auth (Identity dùng cookie mặc định, nhưng bạn có thể set path)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// Authorization (nếu cần policy thì add ở đây)
builder.Services.AddAuthorization();

// =====================
// 2) Razor Components (Blazor Web App)
// =====================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// =====================
// 3) DI khác
// =====================
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// =====================
// 4) Migrate + Seed (PHẢI sau app.Build())
// =====================
await using (var scope = app.Services.CreateAsyncScope())
{
    // Migrate trước (khuyến nghị)
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed duy nhất từ DAL
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    await IdentitySeeder.SeedAsync(roleMgr, userMgr);
}

// =====================
// 5) Middleware
// =====================
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Nếu bạn có dùng antiforgery với form POST:
app.UseAntiforgery();

// =====================
// 6) Map Blazor components
// =====================
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();