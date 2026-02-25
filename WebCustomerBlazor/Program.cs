using DAL.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebCustomerBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

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

app.Run();