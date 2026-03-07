namespace WebAdmin.MVC.Models.Rooms;

public class RoomDetailsVm
{
    public int Id { get; set; }

    public string RoomCode { get; set; } = null!;

    public string? RoomName { get; set; }

    public string Block { get; set; } = "";

    public string Floor { get; set; } = "";

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;
}