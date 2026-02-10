using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Roles;

public class RoleCreateVm
{
    [Required]
    public string Name { get; set; } = null!;
}
