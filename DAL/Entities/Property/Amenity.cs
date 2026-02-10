
using DAL.Entities.Common;

namespace DAL.Entities.Property;

public class Amenity : AuditableEntity<int>
{
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
