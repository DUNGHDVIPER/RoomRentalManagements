using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace WebHostRazor.Pages.Host.Rooms
{
    public class DetailsModel : PageModel
    {
        public RoomModel Room { get; set; }

        public void OnGet(int id)
        {
            // Example data fetching logic
            Room = GetRoomById(id);
        }

        private RoomModel GetRoomById(int id)
        {
            // Replace this with actual data fetching logic
            return new RoomModel
            {
                Id = id,
                Name = "Sample Room",
                District = "Sample District",
                City = "Sample City",
                Area = 50,
                Price = 1000000,
                Status = "Available",
                Images = new List<string>
                {
                    "/images/sample1.jpg",
                    "/images/sample2.jpg",
                    "/images/sample3.jpg"
                }
            };
        }
    }

    public class RoomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public double Area { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public List<string> Images { get; set; }
    }
}
