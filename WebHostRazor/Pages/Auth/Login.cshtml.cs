using BLL.DTOs.Auth;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebHostRazor.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        IAuthService authService,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        ILogger<LoginModel> logger)
    {
        _authService = authService;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    // Properties for the view to bind to
    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? Error { get; set; }
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        Error = null; // Clear any previous errors
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var loginRequest = new LoginRequestDto
            {
                Email = Email,
                Password = Password,
                RememberMe = RememberMe
            };

            var result = await _authService.LoginAsync(loginRequest);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // Redirect admin to WebAdminMVC
                    if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                    {
                        var adminUrl = _configuration["AdminUrl"] ?? "https://localhost:7282";
                        var token = GenerateSimpleToken(user.Email!, roles);
                        return Redirect($"{adminUrl}/Auth/AdminLogin?token={token}");
                    }
                }

                // Normal user redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToPage("/Host/Profile/Index");
            }

            Error = result.Error ?? "Invalid login attempt.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", Email);
            Error = "An error occurred during login. Please try again.";
            return Page();
        }
    }

    private string GenerateSimpleToken(string email, IList<string> roles)
    {
        try
        {
            var tokenData = $"{email}|{string.Join(",", roles)}|{DateTime.UtcNow.AddMinutes(30):yyyy-MM-dd HH:mm:ss}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tokenData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for {Email}", email);
            return string.Empty;
        }
    }
}