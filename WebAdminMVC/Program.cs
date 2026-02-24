using System.Security.Claims;
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

var app = builder.Build();

// =====================
// 6) Middleware pipeline
// =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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