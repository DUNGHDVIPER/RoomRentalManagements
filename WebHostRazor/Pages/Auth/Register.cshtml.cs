using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebHostRazor.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public RegisterFormModel Form { get; set; } = new()
    {
        Email = string.Empty,
        Password = string.Empty,
        ConfirmPassword = string.Empty,
        FullName = string.Empty
    };

    public string? Error { get; set; }
    public string? Success { get; set; }
    public bool IsRegistrationSuccess { get; set; } = false;

    public void OnGet()
    {
        Error = null;
        Success = null;
        IsRegistrationSuccess = false;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Error = null;
        Success = null;
        IsRegistrationSuccess = false;

        if (!ModelState.IsValid)
        {
            return Redirect("/Auth/Login");
        }

        // Check if user exists
        var existingUser = await _userManager.FindByEmailAsync(Form.Email);
        if (existingUser != null)
        {
            Error = "User with this email already exists.";
            return Page();
        }

        // Create new user
        var user = new IdentityUser
        {
            UserName = Form.Email,
            Email = Form.Email,
            PhoneNumber = Form.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, Form.Password);
        if (!result.Succeeded)
        {
            Error = string.Join(", ", result.Errors.Select(e => e.Description));
            return Page();
        }

        // Assign User role
        try
        {
            await _userManager.AddToRoleAsync(user, "User");
        }
        catch (InvalidOperationException)
        {
            Error = "Role assignment failed. Please contact administrator.";
            return Page();
        }

        // Set success flag and message for popup
        IsRegistrationSuccess = true;
        Success = $"Registration successful! Welcome {Form.FullName}. You can now login with your credentials.";

        // Don't auto sign in, let user login manually
        return Page();
    }

    public async Task<IActionResult> OnGetCheckEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new JsonResult(new { available = false });
        }

        var user = await _userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        return new JsonResult(new { available = user == null });
    }
}

public class RegisterFormModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public required string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, lowercase letter, number, and special character")]
    public required string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public required string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
    public required string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [RegularExpression(@"^(\+84|0)[0-9]{9,10}$", ErrorMessage = "Please enter a valid Vietnamese phone number")]
    public string? PhoneNumber { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Terms and Conditions")]
    public bool AcceptTerms { get; set; }
}