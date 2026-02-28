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
                var created = await roleManager.CreateAsync(new IdentityRole(role));
                if (!created.Succeeded)
                {
                    var msg = string.Join("; ", created.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Create role '{role}' failed: {msg}");
                }
            }
        }

        // 2) Users (demo accounts)
        await EnsureUserAsync(userManager, "admin@demo.com", "Admin@123!", "Admin", ct);
        await EnsureUserAsync(userManager, "host@demo.com", "Host@123!", "Host", ct);
        await EnsureUserAsync(userManager, "customer@demo.com", "Customer@123!", "Customer", ct);
        await EnsureUserAsync(userManager, "user@demo.com", "User@123!", "User", ct);
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
        else
        {
            // ✅ DEV ONLY: luôn reset password cho đúng demo password (tránh seed lệch giữa các project)
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, password);
            if (!reset.Succeeded)
            {
                var msg = string.Join("; ", reset.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Reset password '{email}' failed: {msg}");
            }
        }

        // Add role if missing
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