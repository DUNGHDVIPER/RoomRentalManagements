using BLL.DTOs.Auth;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService; // ✅ Thêm dependency

    public AuthService(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IEmailService emailService) // ✅ Inject EmailService
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailService = emailService;
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
        catch (Exception)
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

    // ✅ Implement đầy đủ ForgotPassword
    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Không tiết lộ rằng email không tồn tại (security best practice)
                return new ForgotPasswordResponseDto
                {
                    Succeeded = true,
                    Message = "If your email is registered, you will receive a password reset link shortly."
                };
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create reset link with proper URL encoding
            var resetLink = $"https://localhost:7121/Auth/ResetPassword?email={Uri.EscapeDataString(dto.Email)}&token={Uri.EscapeDataString(token)}";

            // Send email with reset link
            await _emailService.SendPasswordResetEmailAsync(dto.Email, resetLink, ct);

            return new ForgotPasswordResponseDto
            {
                Succeeded = true,
                Message = "If your email is registered, you will receive a password reset link shortly."
            };
        }
        catch (Exception)
        {
            return new ForgotPasswordResponseDto
            {
                Succeeded = false,
                Message = "An error occurred while processing your request. Please try again."
            };
        }
    }

    // ✅ Thêm method ResetPassword
    public async Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new ForgotPasswordResponseDto
                {
                    Succeeded = false,
                    Message = "Invalid request. Please try the forgot password process again."
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ForgotPasswordResponseDto
                {
                    Succeeded = false,
                    Message = errors
                };
            }

            return new ForgotPasswordResponseDto
            {
                Succeeded = true,
                Message = "Your password has been reset successfully. You can now login with your new password."
            };
        }
        catch (Exception)
        {
            return new ForgotPasswordResponseDto
            {
                Succeeded = false,
                Message = "An error occurred while resetting your password. Please try again."
            };
        }
    }

    public Task<AuthResultDto> GoogleLoginStubAsync(string idToken, CancellationToken ct = default)
    {
        // Stub: sau này verify Google token
        return Task.FromResult(new AuthResultDto { Succeeded = false, Error = "Google login not implemented" });
    }
}