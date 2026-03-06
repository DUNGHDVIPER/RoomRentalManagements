using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using WebCustomer.Blazor.Seed;
using WebHostRazor;
using DAL.Data;
using WebHostRazor.Hubs;

=======
using Microsoft.Extensions.FileProviders;
using QuestPDF.Infrastructure;
using DAL.Seed;
using Microsoft.AspNetCore.Identity;
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ✅ Razor Pages (KHÔNG Blazor Server)
builder.Services.AddRazorPages();

// Authorization
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Host", p => p.RequireRole("Host"));
    o.AddPolicy("AdminOrHost", p => p.RequireRole("Admin", "Host", "SuperAdmin"));
});

<<<<<<< HEAD
// Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Host", "Host");
    options.Conventions.AllowAnonymousToFolder("/Auth");
});

builder.Services.AddCascadingAuthenticationState();
// Tenants
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Rooms
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStayHistoryService, StayHistoryService>();

// noti
builder.Services.AddScoped<INotificationService, NotificationService>();
// EF InMemory + Identity (FE-only)
//builder.Services.AddDbContext<AuthDbContext>(opt =>
//    opt.UseInMemoryDatabase("HostPortalAuth"));

//builder.Services
//    .AddIdentity<IdentityUser, IdentityRole>(opt =>
//    {
//        opt.Password.RequireNonAlphanumeric = false;
//        opt.Password.RequiredLength = 6;
//    })
//    .AddEntityFrameworkStores<AuthDbContext>()
//    .AddDefaultTokenProviders();

//builder.Services.AddDbContext<AppDbContext>(opt =>
//    opt.UseInMemoryDatabase("HostPortalApp"));
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
=======
// DbContext
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(cs));
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574

// Identity
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(opt =>
    {
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireDigit = false;
        opt.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.AccessDeniedPath = "/Auth/AccessDenied";
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    opt.SlidingExpiration = true;
});
// SIGNalR
builder.Services.AddSignalR();

// BLL + Repo
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStayHistoryService, StayHistoryService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// CORS (nếu WebAdminMVC gọi chéo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7282", "http://localhost:5220", "https://localhost:5220")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    await IdentitySeeder.SeedAsync(roleMgr, userMgr);
}
// Auto migrate
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// static files
app.UseStaticFiles();

// SharedUploads -> /uploads
var sharedUploadsPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "SharedUploads"));
Directory.CreateDirectory(sharedUploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(sharedUploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseCors("AllowAdminMVC");

app.UseAuthentication();
app.UseAuthorization();
<<<<<<< HEAD
app.MapHub<NotificationHub>("/notificationHub");
// seed roles/users
await app.SeedIdentityAsync();
=======

// Seed roles/users
await SeedIdentityAsync(app);
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574

// ✅ Razor Pages endpoints
app.MapRazorPages();

app.Run();

static async Task SeedIdentityAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Admin", "Host", "SuperAdmin" };
    foreach (var r in roles)
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));

    await EnsureUser(userMgr, "host@demo.com", "Host@123", "Host");
    await EnsureUser(userMgr, "admin@demo.com", "Admin@123", "Admin");
}

static async Task EnsureUser(UserManager<IdentityUser> userMgr, string email, string password, string role)
{
    var user = await userMgr.FindByEmailAsync(email);
    if (user == null)
    {
        user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var created = await userMgr.CreateAsync(user, password);
        if (!created.Succeeded) return;
    }

    if (!await userMgr.IsInRoleAsync(user, role))
        await userMgr.AddToRoleAsync(user, role);
}