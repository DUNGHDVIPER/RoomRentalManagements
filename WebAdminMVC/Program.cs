<<<<<<< HEAD
﻿using BLL.Services;
=======
using BLL.Services;
>>>>>>> origin/main
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
<<<<<<< HEAD
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// 3) BLL services (của bạn)
// =====================
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IReportService, ReportService>();

// =====================
// 4) Session (giống nhóm: có cache + session)
// =====================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
=======
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
>>>>>>> origin/main
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
});

// =====================
// 5) Cookie Authentication (giống nhóm)
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
// 6) Authorization Policy (giống nhóm)
// =====================
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Host", p => p.RequireRole("Host", "Admin"));
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

<<<<<<< HEAD
// Optional: serve SharedUploads as /uploads (giống nhóm)
var sharedUploadsPath = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, "..", "SharedUploads"));
=======
// Optional: serve SharedUploads as /uploads
var sharedUploadsPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "SharedUploads"));
>>>>>>> origin/main
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
// 8) Routing
// =====================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

<<<<<<< HEAD
app.Run();   
=======
app.Run();
>>>>>>> origin/main
