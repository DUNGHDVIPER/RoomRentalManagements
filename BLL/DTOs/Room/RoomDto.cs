namespace BLL.Dtos;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNo { get; set; } = null!;
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
}
