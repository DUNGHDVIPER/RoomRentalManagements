namespace BLL.Services.Interfaces;

public interface IAuditService
{
<<<<<<< HEAD
    Task LogAsync(
        int? actorUserId,
        string action,
        string entityType,
        string entityId,
        string? note = null,
        object? oldValue = null,
        object? newValue = null,
        CancellationToken ct = default);
=======
    Task<AuthResultDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);

    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto dto, CancellationToken ct = default);
    Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default);

    Task<bool> IsEmailExistsAsync(string email, CancellationToken ct = default);
    Task<AuthResultDto> GoogleLoginStubAsync(string idToken, CancellationToken ct = default);
>>>>>>> origin/main
}