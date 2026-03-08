using BLL.DTOs.Auth;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebHostRazor.Pages.Auth;

public class LoginModel(
    IAuthService authService,
    UserManager<IdentityUser> userManager,
    IConfiguration configuration,
    ILogger<LoginModel> logger) : PageModel
{
    private readonly IAuthService _authService = authService;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<LoginModel> _logger = logger;

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
                    _logger.LogInformation("User {Email} logged in with roles: {Roles}", Email, string.Join(",", roles));

                    // Redirect admin to WebAdminMVC
                    if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                    {
                        var adminUrl = _configuration["AdminUrl"] ?? "https://localhost:5220";
                        var token = GenerateSimpleToken(user.Email!, roles);
                        _logger.LogInformation("Redirecting admin to: {AdminUrl}", adminUrl);
                        return Redirect($"{adminUrl}/Auth/AdminLogin?token={token}");
                    }

                    // Redirect users to WebCustomerBlazor (User homepage)
                    if (roles.Contains("User") || roles.Contains("Customer"))
                    {
                        var customerUrl = _configuration["CustomerUrl"] ?? "https://localhost:5000";
                        _logger.LogInformation("Redirecting user to customer portal: {CustomerUrl}", customerUrl);
                        return Redirect($"{customerUrl}");
                    }

                    // Redirect hosts to Host dashboard
                    if (roles.Contains("Host"))
                    {
                        _logger.LogInformation("Redirecting host to dashboard");
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToPage("/Host/Profile/Index");
                    }
                }

                // Default fallback redirect
                _logger.LogWarning("No specific role redirect found for user {Email}, using default", Email);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToPage("/Index"); // Default home page
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