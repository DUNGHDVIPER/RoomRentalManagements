<<<<<<< HEAD
﻿using BLL;
using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
=======
﻿using BLL.Services.Interfaces;
using BLL.Services;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebCustomer.Blazor.Seed;
using WebHostRazor;
using DAL.Data;

>>>>>>> origin/main

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddDal(builder.Configuration);
builder.Services.AddBll(builder.Configuration);

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminMVC", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7282",
            "http://localhost:5220",
            "https://localhost:5220"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Host", p => p.RequireRole("Host"));
    o.AddPolicy("AdminOrHost", p => p.RequireRole("Admin", "Host", "SuperAdmin"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Host", "AdminOrHost");
    options.Conventions.AllowAnonymousToFolder("/Auth");
    options.Conventions.AllowAnonymousToFolder("/Debug");
});

<<<<<<< HEAD
=======
builder.Services.AddCascadingAuthenticationState();
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
>>>>>>> origin/main
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.AccessDeniedPath = "/Auth/AccessDenied";
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    opt.SlidingExpiration = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAdminMVC");

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();