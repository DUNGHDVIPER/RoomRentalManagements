namespace BLL.DTOs.Report;

public class RevenueReportDto
{
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
    public Dictionary<int, decimal> RevenueByMonth { get; set; } = new(); // month -> revenue
    public int OccupiedRooms { get; set; }
    public int TotalRooms { get; set; }
}
