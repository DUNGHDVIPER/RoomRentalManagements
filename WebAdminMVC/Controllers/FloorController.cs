using BLL.DTOs.Property;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAdminMVC.Controllers
{
    public class FloorsController : Controller
    {
        private readonly IFloorService _service;

        public FloorsController(IFloorService service)
        {
            _service = service;
        }

        // LIST
        public async Task<IActionResult> Index(int blockId)
        {
            ViewBag.BlockId = blockId;

            var floors = await _service.GetByBlockAsync(blockId);

            return View(floors);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var floor = await _service.GetByIdAsync(id);

            if (floor == null)
                return NotFound();

            return View(floor);
        }

        // CREATE GET
        public IActionResult Create(int blockId)
        {
            return View(new FloorDto { BlockId = blockId });
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FloorDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _service.CreateAsync(dto);

            return RedirectToAction(nameof(Index), new { blockId = dto.BlockId });
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var floor = await _service.GetByIdAsync(id);

            if (floor == null)
                return NotFound();

            return View(floor);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FloorDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _service.UpdateAsync(dto);

            return RedirectToAction(nameof(Index), new { blockId = dto.BlockId });
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int blockId)
        {
            await _service.DeleteAsync(id);

            return RedirectToAction(nameof(Index), new { blockId });
        }
    }
}