using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace WebHostRazor.Pages.Host.Rooms.Amenities
{
    public class IndexModel : PageModel
    {
        public RoomModel Room { get; set; }

        public void OnGet(int id)
        {
            // Example data retrieval logic
            Room = GetRoomById(id);
        }

        public void OnPost(int id, string name)
        {
            // Example logic to add an amenity
            var room = GetRoomById(id);
            if (room != null && !string.IsNullOrWhiteSpace(name))
            {
                room.Amenities.Add(name);
            }
        }

        public void OnPostRemove(int id, string name)
        {
            // Example logic to remove an amenity
            var room = GetRoomById(id);
            if (room != null)
            {
                room.Amenities.Remove(name);
            }
        }

        private RoomModel GetRoomById(int id)
        {
            // Replace with actual data retrieval logic
            return new RoomModel
            {
                Id = id,
                Name = "Sample Room",
                Amenities = new List<string> { "WiFi", "AC" }
            };
        }
    }

    public class RoomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();
    }
}
