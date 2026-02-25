<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Components.Web;
=======
﻿using DAL.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
>>>>>>> origin/main
using WebCustomerBlazor.Components;
using DAL.Seed;
using BLL;
using BLL.Services;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
builder.Services.AddDal(builder.Configuration);
builder.Services.AddBll(builder.Configuration);
=======
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
>>>>>>> origin/main

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
<<<<<<< HEAD

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

=======

app.UseRouting();            // 👈 BẮT BUỘC

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();        // 👈 BẮT BUỘC (sau Auth)

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<IdentityUser>>();

    var email = "customer@demo.com";
    var password = "Customer@123";

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, password);
    }
}

>>>>>>> origin/main
app.Run();