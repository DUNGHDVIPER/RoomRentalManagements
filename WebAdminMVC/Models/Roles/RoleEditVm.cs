using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Roles;

public class RoleEditVm
{
    [Required]
    public string Name { get; set; } = null!;
}
