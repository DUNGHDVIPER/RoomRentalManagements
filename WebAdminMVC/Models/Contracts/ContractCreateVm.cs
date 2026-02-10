using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Contracts;

public class ContractCreateVm
{
    [Required] public string RoomName { get; set; } = "Room 101";
    [Required] public string TenantName { get; set; } = "Nguyen Van A";
    [Required] public DateTime StartDate { get; set; } = DateTime.Today;
    [Required] public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(6);
    [Required] public string Status { get; set; } = "Active";
    [Range(0, 999999999)] public decimal Deposit { get; set; } = 2000000;
}
