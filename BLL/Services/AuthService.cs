using BLL.DTOs.Auth;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

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

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return new AuthResultDto { Succeeded = false, Error = "Invalid credentials" };

        var res = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: true);
        if (!res.Succeeded) return new AuthResultDto { Succeeded = false, Error = "Invalid credentials" };

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
