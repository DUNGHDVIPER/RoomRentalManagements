using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Auth;

public class ForgotPasswordVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}
