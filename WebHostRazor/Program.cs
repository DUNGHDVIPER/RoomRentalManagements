using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Repositories;
using DAL.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using QuestPDF.Infrastructure;
using WebHostRazor.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Host", "Host");
    options.Conventions.AllowAnonymousToFolder("/Auth");
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Host", p => p.RequireRole("Host"));
    o.AddPolicy("AdminOrHost", p => p.RequireRole("Admin", "Host", "SuperAdmin"));
});

var cs = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(cs, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

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

builder.Services.AddSignalR();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStayHistoryService, StayHistoryService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminMVC", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7282",
                "http://localhost:5220",
                "https://localhost:5220")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

try
{
    await using var scope = app.Services.CreateAsyncScope();

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    logger.LogInformation("Migrating database...");
    await db.Database.MigrateAsync();

    logger.LogInformation("Seeding identity...");
    await IdentitySeeder.SeedAsync(roleMgr, userMgr);

    logger.LogInformation("Database ready.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration failed");
    throw;
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Error");
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
app.UseCors("AllowAdminMVC");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");
app.MapRazorPages();

app.Run();