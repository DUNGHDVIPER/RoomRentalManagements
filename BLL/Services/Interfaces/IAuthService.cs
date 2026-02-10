using BLL.DTOs.Auth;

namespace BLL.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct = default);

    // Google stub (sau này tích hợp OAuth)
    Task<AuthResultDto> GoogleLoginStubAsync(string idToken, CancellationToken ct = default);
}
