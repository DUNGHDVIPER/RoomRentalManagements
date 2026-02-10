using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebHostRazor.Pages.Host.Rooms.Images
{
    public class IndexModel : PageModel
    {
        public RoomModel Room { get; set; }

        public void OnGet()
        {
            // Example initialization for demonstration purposes
            Room = new RoomModel
            {
                Id = 1,
                Name = "Sample Room",
                Images = new List<string> { "image1.jpg", "image2.jpg", "image3.jpg" }
            };
        }

        public void OnPost(int id, string url)
        {
            // Logic to add an image URL to the Room's Images list
            if (Room != null && !string.IsNullOrWhiteSpace(url))
            {
                Room.Images.Add(url);
            }
        }

        public void OnPostRemove(int id, int index)
        {
            // Logic to remove an image by index from the Room's Images list
            if (Room != null && index >= 0 && index < Room.Images.Count)
            {
                Room.Images.RemoveAt(index);
            }
        }
    }

    public class RoomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
