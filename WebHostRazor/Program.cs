using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using WebCustomer.Blazor.Seed;
using WebHostRazor.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Authorization policy "Host"
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Host", p => p.RequireRole("Host"));
});

// Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Host", "Host");
    options.Conventions.AllowAnonymousToFolder("/Auth");
});

QuestPDF.Settings.License = LicenseType.Community;

// ✅ Motel DB (DB thật) => dùng cho nghiệp vụ Contracts/Rooms/...
builder.Services.AddDbContext<MotelManagementDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Identity DB (FE-only) => InMemory (seed users/roles)
builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseInMemoryDatabase("HostPortalAuth"));

// ✅ Identity (RoleManager/UserManager)
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(opt =>
    {
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireDigit = false;
        opt.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Cookie paths
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.AccessDeniedPath = "/Auth/AccessDenied";
});

// ✅ BLL services
builder.Services.Configure<HostOptions>(o =>
{
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// ✅ HostedService
builder.Services.AddHostedService<ContractExpiryReminderHostedService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ✅ seed roles/users (Identity chạy trên AuthDbContext InMemory)
await app.SeedIdentityAsync();

app.MapRazorPages();
app.Run();