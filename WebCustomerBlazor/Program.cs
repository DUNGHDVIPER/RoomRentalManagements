using Microsoft.AspNetCore.Components.Web;
using WebCustomerBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

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
