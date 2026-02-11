namespace BLL.Dtos;

public class RoomDto
{
    public Guid Id { get; set; }
    public required string RoomNo { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public required string Status { get; set; }
}
