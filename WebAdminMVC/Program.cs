using Microsoft.AspNetCore.Mvc;
using DAL; // Thêm using này
using BLL; // Thêm using này

var builder = WebApplication.CreateBuilder(args);

// Thêm DAL và BLL services
builder.Services.AddDal(builder.Configuration);  // ← Thêm dòng này
builder.Services.AddBll(builder.Configuration);  // ← Thêm dòng này

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
app.UseAuthentication(); // ← Thêm dòng này
app.UseAuthorization();  // ← Thêm dòng này

app.UseSession();

// Routing thuần MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();