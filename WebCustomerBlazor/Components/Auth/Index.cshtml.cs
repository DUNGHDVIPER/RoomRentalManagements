using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
namespace WebCustomerBlazor.Components.Auth;

[Authorize(Roles = "User or Tenant")]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public ProfileUpdateModel ProfileForm { get; set; } = new();

    [BindProperty]
    public PasswordChangeModel PasswordForm { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public string AvatarLetter
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ProfileForm.FullName))
            {
                return ProfileForm.FullName.Trim()[0].ToString().ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(ProfileForm.Email))
            {
                return ProfileForm.Email.Trim()[0].ToString().ToUpper();
            }

            return "U";
        }
    }

    public async Task OnGetAsync()
    {
        try
        {
            await LoadUserDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profile data for user {UserName}", User.Identity?.Name);
            ErrorMessage = "Error loading profile data. Please try again.";
        }
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        try
        {
            ClearTempMessages();
            ValidateProfileForm();

            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found when updating profile. Identity name: {UserName}", User.Identity?.Name);
                ErrorMessage = "User not found or session expired. Please log in again.";
                return RedirectToPage("/Auth/Login");
            }

            var normalizedEmail = ProfileForm.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError(
                    $"{nameof(ProfileForm)}.{nameof(ProfileForm.Email)}",
                    "Email address is already in use.");

                await LoadUserDataAsync();
                return Page();
            }

            var updateResult = await UpdateUserInfoAsync(user);
            if (!updateResult.success)
            {
                ErrorMessage = updateResult.error ?? "Unable to update profile.";
                await LoadUserDataAsync();
                return Page();
            }

            // Nạp lại user mới nhất trước khi refresh sign-in
            user = await _userManager.FindByIdAsync(user.Id);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            SuccessMessage = "Profile updated successfully!";
            _logger.LogInformation("Profile updated successfully for user {UserId}", user?.Id);

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserName}", User.Identity?.Name);
            ErrorMessage = "An error occurred while updating your profile. Please try again.";
            await LoadUserDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        try
        {
            ClearTempMessages();
            ValidatePasswordForm();

            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found when changing password. Identity name: {UserName}", User.Identity?.Name);
                ErrorMessage = "User not found or session expired. Please log in again.";
                return RedirectToPage("/Auth/Login");
            }

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, PasswordForm.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                ModelState.AddModelError(
                    $"{nameof(PasswordForm)}.{nameof(PasswordForm.CurrentPassword)}",
                    "Current password is incorrect.");

                await LoadUserDataAsync();
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user,
                PasswordForm.CurrentPassword,
                PasswordForm.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                _logger.LogWarning(
                    "Password change failed for user {UserId}: {Errors}",
                    user.Id,
                    string.Join(", ", changePasswordResult.Errors.Select(e => e.Description)));

                await LoadUserDataAsync();
                return Page();
            }

            // Nạp lại user mới nhất trước khi refresh sign-in
            user = await _userManager.FindByIdAsync(user.Id);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            SuccessMessage = "Password changed successfully!";
            _logger.LogInformation("Password changed successfully for user {UserId}", user?.Id);

            PasswordForm = new PasswordChangeModel();

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserName}", User.Identity?.Name);
            ErrorMessage = "An error occurred while changing your password. Please try again.";
            await LoadUserDataAsync();
            return Page();
        }
    }

    private async Task LoadUserDataAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("LoadUserDataAsync could not find current user. Identity name: {UserName}", User.Identity?.Name);
            return;
        }

        ProfileForm.Email = user.Email ?? string.Empty;
        ProfileForm.PhoneNumber = user.PhoneNumber ?? string.Empty;

        var claims = await _userManager.GetClaimsAsync(user);
        var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");

        if (!string.IsNullOrWhiteSpace(fullNameClaim?.Value))
        {
            ProfileForm.FullName = fullNameClaim.Value;
        }
        else
        {
            ProfileForm.FullName = GetDisplayNameFromEmail(user.Email);
        }
    }

    private async Task<(bool success, string? error)> UpdateUserInfoAsync(IdentityUser user)
    {
        try
        {
            var email = ProfileForm.Email.Trim();
            var fullName = ProfileForm.FullName?.Trim() ?? string.Empty;
            var phoneNumber = string.IsNullOrWhiteSpace(ProfileForm.PhoneNumber)
                ? null
                : ProfileForm.PhoneNumber.Trim();

            var hasUserChanged =
                user.Email != email ||
                user.UserName != email ||
                user.PhoneNumber != phoneNumber;

            if (hasUserChanged)
            {
                user.Email = email;
                user.UserName = email;
                user.PhoneNumber = phoneNumber;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("User update failed for {UserId}: {Errors}", user.Id, errors);
                    return (false, errors);
                }

                // Nạp lại user mới nhất sau UpdateAsync để tránh object cũ
                user = await _userManager.FindByIdAsync(user.Id) ?? user;
            }

            var claimResult = await UpdateFullNameClaimAsync(user, fullName);
            if (!claimResult.success)
            {
                return (false, claimResult.error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user info for user {UserId}", user.Id);
            return (false, "An unexpected error occurred while updating your information.");
        }
    }

    private async Task<(bool success, string? error)> UpdateFullNameClaimAsync(IdentityUser user, string fullName)
    {
        try
        {
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var currentClaim = existingClaims.FirstOrDefault(c => c.Type == "FullName");

            if (string.IsNullOrWhiteSpace(fullName))
            {
                if (currentClaim != null)
                {
                    var removeResult = await _userManager.RemoveClaimAsync(user, currentClaim);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Failed to remove FullName claim for user {UserId}: {Errors}", user.Id, errors);
                        return (false, errors);
                    }
                }

                return (true, null);
            }

            if (currentClaim != null)
            {
                if (currentClaim.Value == fullName)
                {
                    return (true, null);
                }

                var replaceResult = await _userManager.ReplaceClaimAsync(
                    user,
                    currentClaim,
                    new Claim("FullName", fullName));

                if (!replaceResult.Succeeded)
                {
                    var errors = string.Join(", ", replaceResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to replace FullName claim for user {UserId}: {Errors}", user.Id, errors);
                    return (false, errors);
                }
            }
            else
            {
                var addResult = await _userManager.AddClaimAsync(user, new Claim("FullName", fullName));
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to add FullName claim for user {UserId}: {Errors}", user.Id, errors);
                    return (false, errors);
                }
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update FullName claim for user {UserId}", user.Id);
            return (false, "Failed to update full name.");
        }
    }

    private void ValidateProfileForm()
    {
        RemoveModelStateKeysStartingWith($"{nameof(PasswordForm)}.");

        if (string.IsNullOrWhiteSpace(ProfileForm.Email))
        {
            ModelState.AddModelError(
                $"{nameof(ProfileForm)}.{nameof(ProfileForm.Email)}",
                "Email is required.");
        }

        if (!string.IsNullOrWhiteSpace(ProfileForm.Email) &&
            !new EmailAddressAttribute().IsValid(ProfileForm.Email))
        {
            ModelState.AddModelError(
                $"{nameof(ProfileForm)}.{nameof(ProfileForm.Email)}",
                "Please enter a valid email address.");
        }

        if (!string.IsNullOrWhiteSpace(ProfileForm.FullName) && ProfileForm.FullName.Length > 100)
        {
            ModelState.AddModelError(
                $"{nameof(ProfileForm)}.{nameof(ProfileForm.FullName)}",
                "Full name cannot exceed 100 characters.");
        }

        if (!string.IsNullOrWhiteSpace(ProfileForm.PhoneNumber) && ProfileForm.PhoneNumber.Length > 20)
        {
            ModelState.AddModelError(
                $"{nameof(ProfileForm)}.{nameof(ProfileForm.PhoneNumber)}",
                "Phone number must not exceed 20 characters.");
        }
    }

    private void ValidatePasswordForm()
    {
        RemoveModelStateKeysStartingWith($"{nameof(ProfileForm)}.");

        if (string.IsNullOrWhiteSpace(PasswordForm.CurrentPassword))
        {
            ModelState.AddModelError(
                $"{nameof(PasswordForm)}.{nameof(PasswordForm.CurrentPassword)}",
                "Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(PasswordForm.NewPassword))
        {
            ModelState.AddModelError(
                $"{nameof(PasswordForm)}.{nameof(PasswordForm.NewPassword)}",
                "New password is required.");
        }

        if (string.IsNullOrWhiteSpace(PasswordForm.ConfirmNewPassword))
        {
            ModelState.AddModelError(
                $"{nameof(PasswordForm)}.{nameof(PasswordForm.ConfirmNewPassword)}",
                "Please confirm your new password.");
        }

        if (!string.IsNullOrWhiteSpace(PasswordForm.NewPassword) && PasswordForm.NewPassword.Length < 8)
        {
            ModelState.AddModelError(
                $"{nameof(PasswordForm)}.{nameof(PasswordForm.NewPassword)}",
                "Password must be at least 8 characters long.");
        }

        if (!string.IsNullOrWhiteSpace(PasswordForm.NewPassword) &&
            PasswordForm.NewPassword == PasswordForm.CurrentPassword)
        {
            ModelState.AddModelError(
                $"{nameof(PasswordForm)}.{nameof(PasswordForm.NewPassword)}",
                "New password must be different from current password.");
        }

        if (PasswordForm.NewPassword != PasswordForm.ConfirmNewPassword)
        {
            ModelState.AddModelError(
                $"{nameof(PasswordForm)}.{nameof(PasswordForm.ConfirmNewPassword)}",
                "Passwords do not match.");
        }
    }

    private void RemoveModelStateKeysStartingWith(string prefix)
    {
        var keysToRemove = ModelState.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }
    }

    private void ClearTempMessages()
    {
        SuccessMessage = null;
        ErrorMessage = null;
    }

    private static string GetDisplayNameFromEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        var atIndex = email.IndexOf('@');
        return atIndex > 0 ? email[..atIndex] : email;
    }
}

public class ProfileUpdateModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Full name must not exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
}

public class PasswordChangeModel
{
    [Required(ErrorMessage = "Current password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}