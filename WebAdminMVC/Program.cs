using Microsoft.AspNetCore.Identity;
using DAL.Seed;
using DAL.Data;
using BLL;
using BLL.Services;
using BLL.Services.Interfaces;

try
{
    Console.WriteLine("Starting WebAdminMVC application...");

    var builder = WebApplication.CreateBuilder(args);

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
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
    Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
    Environment.Exit(-1);
}