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

// Cookie Auth
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// Authorization
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// =====================
// 2) Razor Components
// =====================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// =====================
// 3) Services DI
// =====================
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// =====================
// 4) Migrate + Seed
// =====================
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    await IdentitySeeder.SeedAsync(roleMgr, userMgr);

    // Reset password Admin
    var admin = await userMgr.FindByEmailAsync("admin@demo.com");

    if (admin != null)
    {
        var token = await userMgr.GeneratePasswordResetTokenAsync(admin);
        await userMgr.ResetPasswordAsync(admin, token, "Admin@123");

        await userMgr.SetLockoutEndDateAsync(admin, null);
        admin.AccessFailedCount = 0;
        await userMgr.UpdateAsync(admin);
    }
}

// =====================
// 5) Middleware
// =====================
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// =====================
// 6) Map Blazor
// =====================
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();