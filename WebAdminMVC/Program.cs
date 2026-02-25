<<<<<<< HEAD
﻿using System.Security.Claims;
using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// =====================
// 1) MVC
// =====================
builder.Services.AddControllersWithViews();

// =====================
// 2) DAL DbContext
// =====================
builder.Services.AddDbContext<MotelManagementDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =====================
// 3) BLL services
// =====================
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Nếu ContractService cần thêm service khác (Audit/Notification...) thì đăng ký thêm ở đây.
// builder.Services.AddScoped<IAuditService, AuditService>();

// =====================
// 4) Cookie Authentication (tự viết)
// =====================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
        opt.AccessDeniedPath = "/Account/AccessDenied";
        opt.SlidingExpiration = true;

        // Tuỳ chọn:
        // opt.ExpireTimeSpan = TimeSpan.FromHours(8);
        // opt.Cookie.Name = "WebAdmin.Auth";
    });

// =====================
// 5) Authorization (Roles/Policy)
// =====================
builder.Services.AddAuthorization(opt =>
{
    // Bạn có thể dùng [Authorize(Roles="Admin,Host")] trực tiếp
    // hoặc dùng policy Host
    opt.AddPolicy("Host", p => p.RequireRole("Host", "Admin"));
});
=======
﻿using Microsoft.AspNetCore.Identity;
using DAL.Seed;
using DAL.Data;
using BLL;
using BLL.Services;
using BLL.Services.Interfaces;

try
{
    Console.WriteLine("Starting WebAdminMVC application...");
>>>>>>> origin/main

    var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// =====================
// 6) Middleware pipeline
// =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
=======
    // ✅ Enhanced logging for debugging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

    Console.WriteLine("Configuring services...");

    // Test connection string first
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Connection string: {connectionString}");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' is null or empty.");
    }

    // Add DAL and BLL services
    Console.WriteLine("Adding DAL services...");
    builder.Services.AddDal(builder.Configuration);

    Console.WriteLine("Adding BLL services...");
    builder.Services.AddBll(builder.Configuration);

    // Register EmailService
    Console.WriteLine("Registering EmailService...");
    builder.Services.AddScoped<IEmailService, EmailService>();

    // Configure Identity token lifespan
    builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
    {
        opt.TokenLifespan = TimeSpan.FromHours(1);
    });

    // Authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
        options.AddPolicy("HostOrAdmin", policy => policy.RequireRole("Admin", "SuperAdmin", "Host"));
    });

    Console.WriteLine("Adding MVC services...");
    builder.Services.AddControllersWithViews();

    builder.Services.AddSession(opt =>
    {
        opt.IdleTimeout = TimeSpan.FromHours(8);
        opt.Cookie.HttpOnly = true;
        opt.Cookie.IsEssential = true;
    });

    Console.WriteLine("Building application...");
    var app = builder.Build();

    Console.WriteLine("Configuring middleware pipeline...");

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        Console.WriteLine("Using Developer Exception Page");
    }
    else
    {
        app.UseExceptionHandler("/Auth/AccessDenied");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSession();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    Console.WriteLine("Application configured successfully. Starting web server...");

    // Test database connection before starting
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.CanConnectAsync();
            Console.WriteLine("Database connection successful!");
        }
        catch (Exception dbEx)
        {
            Console.WriteLine($"Database connection failed: {dbEx.Message}");
            throw;
        }
    }

    app.Run();
>>>>>>> origin/main
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
    Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

<<<<<<< HEAD
app.UseHttpsRedirection();

// Serve wwwroot => để /uploads/contracts/... truy cập được
app.UseStaticFiles();
var sharedUploadsPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "SharedUploads"));
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
// 7) Routing
// =====================
// Default route cho MVC view controllers: /{controller=Home}/{action=Index}/{id?}
app.MapDefaultControllerRoute();

// Nếu bạn dùng attribute routing nhiều ([Route("host/contracts")]) thì MapDefaultControllerRoute vẫn OK,
// nhưng để chắc chắn, bạn có thể thêm dòng này:
app.MapControllers();

app.Run();
=======
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
    Environment.Exit(-1);
}
>>>>>>> origin/main
