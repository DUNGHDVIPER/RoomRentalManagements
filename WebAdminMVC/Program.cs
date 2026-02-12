using Microsoft.AspNetCore.Mvc;
using DAL.Seed; // Thay đổi từ DAL thành DAL.Seed
using BLL; // BLL OK

var builder = WebApplication.CreateBuilder(args);

// Thêm DAL và BLL services
try
{
    builder.Services.AddDal(builder.Configuration);
    builder.Services.AddBll(builder.Configuration);
}
catch (Exception ex)
{
    Console.WriteLine($"Error configuring services: {ex.Message}");
    throw;
}

builder.Services.AddControllersWithViews();

// Fake auth (Session)
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(8);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/AccessDenied");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thêm middleware authentication và authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Routing thuần MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();