namespace WebAdmin.MVC.Models.Contracts;

public class ContractDetailsVm
{
    public int Id { get; set; }
    public string RoomName { get; set; } = null!;
    public string TenantName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Deposit { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow.AddDays(-30);
}
