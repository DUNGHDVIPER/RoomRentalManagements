namespace WebAdmin.MVC.Models.Users;

public class UserListItemVm
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Status { get; set; } = "Active"; // Active / Locked
}
