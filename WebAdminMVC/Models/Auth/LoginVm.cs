using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Auth;

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
