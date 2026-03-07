using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using QuestPDF.Infrastructure;
using DAL.Seed;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ✅ Razor Pages
builder.Services.AddRazorPages();

// Authorization
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Host", p => p.RequireRole("Host"));
    o.AddPolicy("AdminOrHost", p => p.RequireRole("Admin", "Host", "SuperAdmin"));
});

// DbContext with retry logic for transient failures
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(cs, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

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

// BLL + Repo
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStayHistoryService, StayHistoryService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// CORS
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

// Database migration and seeding with proper error handling
try
{
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        logger.LogInformation("Checking database connection...");

        if (!await db.Database.CanConnectAsync())
        {
            logger.LogInformation("Database not accessible, creating database...");
            await db.Database.EnsureCreatedAsync();
        }

        logger.LogInformation("Starting database migration...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully.");

        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        logger.LogInformation("Starting identity seeding...");
        await IdentitySeeder.SeedAsync(roleMgr, userMgr);
        logger.LogInformation("Identity seeding completed successfully.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database migration or seeding");

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine($"Database Error: {ex.Message}");
        Console.WriteLine("Make sure your SQL Server is running and the connection string is correct.");
        throw;
    }
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

// ✅ Razor Pages endpoints
app.MapRazorPages();

app.Run();