using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebHostRazor.Pages.Host.Rooms
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public RoomFormModel Form { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Handle form submission logic here (e.g., save to database)

            return RedirectToPage("/Host/Rooms");
        }
    }

    public class RoomFormModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        public string District { get; set; }

        [Range(1, 10000)]
        public double Area { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
