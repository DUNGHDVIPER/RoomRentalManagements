using BLL.Services.Interfaces;
using BLL.Services;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebCustomer.Blazor.Seed;
using WebHostRazor;
using DAL.Data;


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
// Tenants
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Rooms
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStayHistoryService, StayHistoryService>();
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

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(opt =>
    {
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.AccessDeniedPath = "/Auth/AccessDenied";
});

// FE mock store


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// seed roles/users
await app.SeedIdentityAsync();

app.MapRazorPages();
app.Run();
