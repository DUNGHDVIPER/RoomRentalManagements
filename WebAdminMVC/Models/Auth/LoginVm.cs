using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Auth;

public class LoginVm
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    // Chọn role để test nhanh
    [Required]
    public string Role { get; set; } = "Host";

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}