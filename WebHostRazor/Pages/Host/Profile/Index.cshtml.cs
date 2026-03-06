using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebHostRazor.Pages.Host.Profile;

public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public IndexModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public ProfileUpdateModel ProfileForm { get; set; } = new();

    [BindProperty]
    public PasswordChangeModel PasswordForm { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ProfileForm.Email = user.Email ?? string.Empty;
                ProfileForm.PhoneNumber = user.PhoneNumber ?? string.Empty;

                // Optimize: Get display name from claims without additional DB call
                var fullNameClaim = User.FindFirst("FullName");
                ProfileForm.FullName = fullNameClaim?.Value ?? GetDisplayNameFromEmail(user.Email);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error loading profile data. Please try again.";
            // Log the exception if you have logging configured
        }
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            // Update user information
            user.Email = ProfileForm.Email;
            user.UserName = ProfileForm.Email;
            user.PhoneNumber = ProfileForm.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                await LoadUserDataAsync();
                return Page();
            }

            // Update display name claim - optimize by doing both operations together
            if (!string.IsNullOrWhiteSpace(ProfileForm.FullName))
            {
                await UpdateFullNameClaimAsync(user, ProfileForm.FullName);
            }

            SuccessMessage = "Profile updated successfully!";
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while updating your profile. Please try again.";
            await LoadUserDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            // Clear profile form validation errors
            ModelState.Remove("ProfileForm.Email");
            ModelState.Remove("ProfileForm.FullName");

            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user, PasswordForm.CurrentPassword, PasswordForm.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                ErrorMessage = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
                await LoadUserDataAsync();
                return Page();
            }

            // Refresh the sign-in to update security stamp
            await _signInManager.RefreshSignInAsync(user);

            SuccessMessage = "Password changed successfully!";

            // Clear password form
            PasswordForm = new PasswordChangeModel();

            await LoadUserDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while changing your password. Please try again.";
            await LoadUserDataAsync();
            return Page();
        }
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ProfileForm.Email = user.Email ?? string.Empty;
                ProfileForm.PhoneNumber = user.PhoneNumber ?? string.Empty;

                // Get from current claims first to avoid DB call
                var fullNameClaim = User.FindFirst("FullName");
                ProfileForm.FullName = fullNameClaim?.Value ?? GetDisplayNameFromEmail(user.Email);
            }
        }
        catch (Exception)
        {
            // Handle gracefully
            ProfileForm = new ProfileUpdateModel();
        }
    }

    private async Task UpdateFullNameClaimAsync(IdentityUser user, string fullName)
    {
        try
        {
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var nameClaim = existingClaims.FirstOrDefault(c => c.Type == "FullName");

            var claimsToUpdate = new List<Claim>();
            if (nameClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, nameClaim);
            }

            await _userManager.AddClaimAsync(user, new Claim("FullName", fullName));
        }
        catch (Exception)
        {
            // If claim update fails, don't break the profile update
        }
    }

    private static string GetDisplayNameFromEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "User";

        var atIndex = email.IndexOf('@');
        return atIndex > 0 ? email[..atIndex] : "User";
    }
}

public class ProfileUpdateModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Full name must not exceed 50 characters")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [RegularExpression(@"^(\+84|0)[0-9]{9,10}$", ErrorMessage = "Please enter a valid Vietnamese phone number")]
    public string? PhoneNumber { get; set; }
}

public class PasswordChangeModel
{
    [Required(ErrorMessage = "Current password is required")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, lowercase letter, number, and special character")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}