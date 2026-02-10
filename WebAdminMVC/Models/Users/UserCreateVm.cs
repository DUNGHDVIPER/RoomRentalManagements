using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Users;

public class UserCreateVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Role { get; set; } = "Customer";

    [Required]
    public string Status { get; set; } = "Active";
}
