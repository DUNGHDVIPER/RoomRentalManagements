namespace DAL.Entities.Contracts;

public class Deposit
{
    public long DepositId { get; set; }
    public long ContractId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Hold";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public Contract Contract { get; set; } = null!;
}