using System.ComponentModel.DataAnnotations;

namespace WebAdmin.MVC.Models.Rooms;

public class RoomCreateVm
{
    [Required]
    public string RoomCode { get; set; } = null!;

    public string? RoomName { get; set; }

    [Required]
    public int FloorId { get; set; }

    public decimal? AreaM2 { get; set; }

    public int MaxOccupants { get; set; } = 2;

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    [Required]
    public string Status { get; set; } = "Available";

    public string? Description { get; set; }
}