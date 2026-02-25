using BLL.DTOs.Auth;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebHostRazor.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<ResetPasswordModel> _logger;

    [BindProperty]
    public ResetPasswordRequestDto ResetPasswordRequest { get; set; } = new();

    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public bool ResetCompleted { get; set; }

    public ResetPasswordModel(IAuthService authService, ILogger<ResetPasswordModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public IActionResult OnGet(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            Message = "Invalid reset link. Please try the forgot password process again.";
            IsSuccess = false;
            return Page();
        }

        ResetPasswordRequest.Email = email;
        ResetPasswordRequest.Token = token;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _authService.ResetPasswordAsync(ResetPasswordRequest);

            Message = result.Message;
            IsSuccess = result.Succeeded;
            ResetCompleted = result.Succeeded;

            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset completed for email: {Email}", ResetPasswordRequest.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            Message = "An error occurred. Please try again.";
            IsSuccess = false;
        }

        return Page();
    }
}