using Microsoft.AspNetCore.Components.Web;
using WebCustomerBlazor.Components;
using DAL; // Thêm using này
using BLL; // Thêm using này

var builder = WebApplication.CreateBuilder(args);

// Thêm DAL và BLL services
builder.Services.AddDal(builder.Configuration);  // ← Thêm dòng này
builder.Services.AddBll(builder.Configuration);  // ← Thêm dòng này

// Blazor Web App (.NET 8)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map root component
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();