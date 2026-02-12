using BLL.DTOs.Auth;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthService(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        try
        {
            // Check if user already exists
            if (await IsEmailExistsAsync(dto.Email, ct))
            {
                return new AuthResultDto
                {
                    Succeeded = false,
                    Error = "An account with this email already exists"
                };
            }

            // Create new user
            var user = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = false // Require email confirmation in production
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResultDto { Succeeded = false, Error = errors };
            }

            // Assign Host role by default
            var roleResult = await _userManager.AddToRoleAsync(user, "Host");
            if (!roleResult.Succeeded)
            {
                // Rollback user creation if role assignment fails
                await _userManager.DeleteAsync(user);
                return new AuthResultDto
                {
                    Succeeded = false,
                    Error = "Failed to assign user role. Please try again."
                };
            }

            // Auto sign in after registration
            await _signInManager.SignInAsync(user, isPersistent: false);

            var roles = await _userManager.GetRolesAsync(user);
            return new AuthResultDto
            {
                Succeeded = true,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToArray(),
                Message = "Registration successful! Welcome to Host Portal."
            };
        }
        catch (Exception ex)
        {
            return new AuthResultDto
            {
                Succeeded = false,
                Error = "An error occurred during registration. Please try again."
            };
        }
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return new AuthResultDto { Succeeded = false, Error = "Invalid email or password" };

        var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return new AuthResultDto { Succeeded = false, Error = "Account is locked. Please try again later." };

        if (!result.Succeeded)
            return new AuthResultDto { Succeeded = false, Error = "Invalid email or password" };

        var roles = await _userManager.GetRolesAsync(user);
        return new AuthResultDto
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email,
            Roles = roles.ToArray()
        };
    }

    public Task LogoutAsync(CancellationToken ct = default)
        => _signInManager.SignOutAsync();

    public Task ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct = default)
    {
        // Stub: sau này generate token + email sender
        return Task.CompletedTask;
    }

    public Task<AuthResultDto> GoogleLoginStubAsync(string idToken, CancellationToken ct = default)
    {
        // Stub: sau này verify Google token
        return Task.FromResult(new AuthResultDto { Succeeded = false, Error = "Google login not implemented" });
    }
}