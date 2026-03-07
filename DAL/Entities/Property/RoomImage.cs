using DAL.Entities.Common;

namespace DAL.Entities.Property;

public class RoomImage : AuditableEntity<int>
{
    public int RoomId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }

    public Room Room { get; set; } = null!;
}