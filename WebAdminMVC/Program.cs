<<<<<<< HEAD
﻿using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BLL.Services.Interfaces;
using BLL.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<INotificationService, NotificationService>();


// ✅ ADD IDENTITY
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
=======
using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// =====================
// 1) MVC
// =====================
builder.Services.AddControllersWithViews();

// =====================
// 2) DbContext (DAL)
// =====================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

/*builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));*/

// =====================
// 3) BLL services
// =====================
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// (Nếu bạn có EmailService dùng trong WebAdminMVC thì mở dòng này)
// builder.Services.AddScoped<IEmailService, EmailService>();

// =====================
// 4) Session
// =====================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =====================
// 5) Cookie Authentication (tự viết)
// =====================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
        opt.AccessDeniedPath = "/Account/AccessDenied";
        opt.SlidingExpiration = true;
        // opt.ExpireTimeSpan = TimeSpan.FromHours(8);
        // opt.Cookie.Name = "WebAdmin.Auth";
    });

// =====================
// 6) Authorization
// =====================
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Host", p => p.RequireRole("Host", "Admin"));
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574
});

var app = builder.Build();

// =====================
// 7) Middleware pipeline
// =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// wwwroot static files
app.UseStaticFiles();

// Optional: serve SharedUploads as /uploads
var sharedUploadsPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "SharedUploads"));
Directory.CreateDirectory(sharedUploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(sharedUploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();

// ❌ BỎ Session fake auth
// app.UseSession();

// ✅ BẮT BUỘC
app.UseAuthentication();
app.UseAuthorization();

<<<<<<< HEAD
=======
app.UseAuthentication();
app.UseAuthorization();

// =====================
// 8) Routing
// =====================
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

<<<<<<< HEAD

// ✅ Seed Role
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Host", "Tenant" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

=======
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574
app.Run();