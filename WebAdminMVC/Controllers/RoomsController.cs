using BLL.DTOs.Room;
using BLL.Services.Interfaces;
using DAL.Entities.Common;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.MVC.Models.Rooms;

namespace WebAdmin.MVC.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // ===================== LIST =====================

    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllAsync();

        var vm = rooms.Select(r => new RoomListItemVm
        {
            Id = r.RoomId,
            Name = r.RoomName ?? r.RoomCode,
            Block = r.BlockName ?? "",
            Floor = r.FloorNumber?.ToString() ?? "",
            Price = r.CurrentBasePrice,
            Status = r.Status.ToString()
        }).ToList();

        return View(vm);
    }

    // ===================== CREATE =====================

    [HttpGet]
    public IActionResult Create()
    {
        return View(new RoomCreateVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomCreateVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var dto = new CreateRoomDto
        {
            FloorId = vm.FloorId,
            RoomCode = vm.RoomCode,
            RoomName = vm.RoomName,
            AreaM2 = vm.AreaM2,
            MaxOccupants = vm.MaxOccupants,
            CurrentBasePrice = vm.Price,
            Status = Enum.Parse<RoomStatus>(vm.Status),
            Description = vm.Description,
            AmenityIds = Array.Empty<int>()
        };

        await _roomService.CreateRoomAsync(dto);

        return RedirectToAction(nameof(Index));
    }

    // ===================== EDIT =====================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetRoomDetailAsync(id);

        if (room == null)
            return NotFound();

        var vm = new RoomEditVm
        {
            Id = room.RoomId,
            RoomCode = room.RoomCode,
            RoomName = room.RoomName,
            AreaM2 = room.AreaM2,
            MaxOccupants = room.MaxOccupants,
            CurrentBasePrice = room.CurrentBasePrice,
            Status = room.Status,
            Description = room.Description
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoomEditVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var dto = new UpdateRoomDto
        {
            RoomCode = vm.RoomCode,
            RoomName = vm.RoomName,
            AreaM2 = vm.AreaM2,
            MaxOccupants = vm.MaxOccupants,
            CurrentBasePrice = vm.CurrentBasePrice,
            Status = vm.Status,
            Description = vm.Description,
            AmenityIds = Array.Empty<int>()
        };

        await _roomService.UpdateRoomAsync(vm.Id, dto);

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Details(int id)
    {
        var room = await _roomService.GetRoomDetailAsync(id);

        if (room == null)
            return NotFound();

        var vm = new RoomDetailsVm
        {
            Id = room.RoomId,
            RoomCode = room.RoomCode,
            RoomName = room.RoomName,
            Block = room.BlockName ?? "",
            Floor = room.FloorNumber?.ToString() ?? "",
            Price = room.CurrentBasePrice,
            Status = room.Status.ToString()
        };

        return View(vm);
    }
    // ===================== DELETE =====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _roomService.DeleteRoomAsync(id);

        return RedirectToAction(nameof(Index));
    }
}