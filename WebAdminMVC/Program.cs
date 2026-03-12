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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// 3) Identity (Single Authentication System)
// =====================
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
});

// =====================
// 4) BLL Services
// =====================
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IFloorRepository, FloorRepository>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
// (Nếu bạn có EmailService dùng trong WebAdminMVC thì mở dòng này)
// builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IRoomService, RoomService>();
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

// =====================
// 6) Authorization - Using AddAuthorizationBuilder
// =====================
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"))
    .AddPolicy("AdminOrHost", policy => policy.RequireRole("Admin", "SuperAdmin", "Host"));

var app = builder.Build();

// =====================
// INITIALIZE DATABASE AND SEED DATA
// =====================
await InitializeDatabaseAsync(app.Services);

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

// serve SharedUploads as /uploads
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

app.Run();

// =====================
// DATABASE INITIALIZATION METHOD
// =====================
static async Task InitializeDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Create roles
        string[] roles = ["SuperAdmin", "Admin", "Host", "Tenant"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }

        // Create default admin user
        var adminEmail = "admin@system.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                logger.LogInformation("Created default admin user: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Reset admin password to ensure it matches demo credentials
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, token, "Admin@123456");

            if (resetResult.Succeeded)
            {
                // Ensure admin has SuperAdmin role
                var userRoles = await userManager.GetRolesAsync(adminUser);
                if (!userRoles.Contains("SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
                logger.LogInformation("Reset admin password and verified roles");
            }
        }

        // Create demo host user
        var hostEmail = "host@demo.com";
        var hostUser = await userManager.FindByEmailAsync(hostEmail);

        if (hostUser == null)
        {
            hostUser = new IdentityUser
            {
                UserName = hostEmail,
                Email = hostEmail,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await userManager.CreateAsync(hostUser, "Host@123");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(hostUser, "Host");
                logger.LogInformation("Created demo host user: {Email}", hostEmail);
            }
        }
        else
        {
            // Reset host password
            var token = await userManager.GeneratePasswordResetTokenAsync(hostUser);
            await userManager.ResetPasswordAsync(hostUser, token, "Host@123");
            logger.LogInformation("Reset host password");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing database");
    }
}