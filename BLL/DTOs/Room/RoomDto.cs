namespace BLL.Dtos;

public class RoomDto
{
<<<<<<< HEAD
    public Guid Id { get; set; }
    public required string RoomNo { get; set; }
=======
    public int Id { get; set; }// doi Guid thanh int de cho giong voi DAL
    public string RoomNo { get; set; } = null!;
>>>>>>> origin/main
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public required string Status { get; set; }
}
