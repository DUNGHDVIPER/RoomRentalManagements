using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using QuestPDF.Infrastructure;
using WebHostRazor.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Host", "Host");
    options.Conventions.AllowAnonymousToFolder("/Auth");
});

// Authorization
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Host", p => p.RequireRole("Host"));
    o.AddPolicy("AdminOrHost", p => p.RequireRole("Admin", "Host", "SuperAdmin"));
});

// DbContext
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(cs));

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

// SignalR
builder.Services.AddSignalR();

// BLL + Repository
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStayHistoryService, StayHistoryService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// CORS (cho WebAdminMVC)
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

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// Auto migrate database
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

// Static files
app.UseStaticFiles();

// SharedUploads -> /uploads
var sharedUploadsPath = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, "..", "SharedUploads"));

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

// SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

// Razor Pages
app.MapRazorPages();

// Seed roles + demo users
await SeedIdentityAsync(app);

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
        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var created = await userMgr.CreateAsync(user, password);
        if (!created.Succeeded) return;
    }

    if (!await userMgr.IsInRoleAsync(user, role))
        await userMgr.AddToRoleAsync(user, role);
}