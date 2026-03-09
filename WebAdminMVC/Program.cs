using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// =====================
// 1) MVC
// =====================
builder.Services.AddControllersWithViews();

// =====================
// 2) DbContext
// =====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// 3) Identity
// =====================
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// =====================
// 4) BLL Services
// =====================
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// =====================
// 5) Session
// =====================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// =====================
// RESET ADMIN PASSWORD
// =====================
using (var scope = app.Services.CreateScope())
{
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var user = await userMgr.FindByEmailAsync("admin@demo.com");

    if (user != null)
    {
        var token = await userMgr.GeneratePasswordResetTokenAsync(user);
        await userMgr.ResetPasswordAsync(user, token, "Admin@12345!");
    }
}


// =====================
// Middleware
// =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

var sharedUploadsPath = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, "..", "SharedUploads"));

Directory.CreateDirectory(sharedUploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(sharedUploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


// =====================
// Routing
// =====================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");


// =====================
// Seed Roles
// =====================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Host", "Tenant" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.Run();