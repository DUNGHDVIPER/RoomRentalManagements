using Microsoft.AspNetCore.Identity;

namespace DAL.Seed;

public static class IdentitySeeder
{
    public static readonly string[] Roles = ["Admin", "Host", "Customer", "User"];

    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        CancellationToken ct = default)
    {
        // 1) Roles
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2) Users
        await EnsureUserAsync(userManager, "admin@demo.com", "Admin@12345!", "Admin", ct);
        await EnsureUserAsync(userManager, "host@demo.com", "Host@12345!", "Host", ct);
        await EnsureUserAsync(userManager, "customer@demo.com", "Customer@12345!", "Customer", ct);
        await EnsureUserAsync(userManager, "user@demo.com", "User@12345!", "User", ct);
    }

    private static async Task EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                var msg = string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Create user '{email}' failed: {msg}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var addRole = await userManager.AddToRoleAsync(user, role);
            if (!addRole.Succeeded)
            {
                var msg = string.Join("; ", addRole.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Add role '{role}' to '{email}' failed: {msg}");
            }
        }
    }
}
