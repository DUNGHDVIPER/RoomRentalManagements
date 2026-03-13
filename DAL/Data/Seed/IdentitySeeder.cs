using Microsoft.AspNetCore.Identity;

namespace DAL.Seed;

public static class IdentitySeeder
{
    public static readonly string[] Roles = ["Admin", "Host", "Customer", "Tenant"];

    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        CancellationToken ct = default)
    {
        foreach (var role in Roles)
        {
            ct.ThrowIfCancellationRequested();

            if (!await roleManager.RoleExistsAsync(role))
            {
                var created = await roleManager.CreateAsync(new IdentityRole(role));
                if (!created.Succeeded)
                {
                    var msg = string.Join("; ", created.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Create role '{role}' failed: {msg}");
                }
            }
        }

        await EnsureUserAsync(userManager, "admin@demo.com", "Admin", "Admin@123!", ct);
        await EnsureUserAsync(userManager, "host@demo.com", "Host", "Host@123!", ct);
        await EnsureUserAsync(userManager, "customer@demo.com", "Customer", "Customer@123!", ct);
        await EnsureUserAsync(userManager, "tenant@demo.com", "Tenant", "Tenant@123!", ct);
    }

    private static async Task EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string role,
        string password,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                var msg = string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Create user '{email}' failed: {msg}");
            }
        }

        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Contains(role))
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