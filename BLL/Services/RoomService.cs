using BLL.Common;
using BLL.DTOs;
using BLL.DTOs.Property;
using BLL.DTOs.Room;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Entities.Property;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;

    public RoomService(AppDbContext context)
    {
        _context = context;
    }

    // ================== ROOMS ==================

    public async Task<List<RoomDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Select(r => MapToDto(r))
            .ToListAsync(ct);
    }

    public async Task<RoomDto> GetRoomDetailAsync(int roomId, CancellationToken ct = default)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoomId == roomId, ct);

        if (room == null)
            throw new Exception("Room not found");

        return MapToDto(room);
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, CancellationToken ct = default)
    {
        var room = new Room
        {
            FloorId = dto.FloorId,
            RoomCode = dto.RoomCode,
            RoomName = dto.RoomName,
            AreaM2 = dto.AreaM2,
            MaxOccupants = dto.MaxOccupants,
            Status = dto.Status,
            CurrentBasePrice = dto.CurrentBasePrice,
            Description = dto.Description
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(ct);

        return MapToDto(room);
    }

    public async Task<RoomDto> UpdateRoomAsync(int roomId, UpdateRoomDto dto, CancellationToken ct = default)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId, ct);
        if (room == null)
            throw new Exception("Room not found");

        room.RoomName = dto.RoomName;
        room.AreaM2 = dto.AreaM2;
        room.MaxOccupants = dto.MaxOccupants;
        room.Status = dto.Status;
        room.CurrentBasePrice = dto.CurrentBasePrice;
        room.Description = dto.Description;

        await _context.SaveChangesAsync(ct);
        return MapToDto(room);
    }

    public async Task DeleteRoomAsync(int roomId, CancellationToken ct = default)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId, ct);
        if (room == null) return;

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(ct);
    }

    // ================== NOT IMPLEMENTED YET ==================

    public Task<List<BlockDto>> GetBlocksAsync(CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<BlockDto> CreateBlockAsync(BlockDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<BlockDto> UpdateBlockAsync(int id, BlockDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task DeleteBlockAsync(int id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<List<FloorDto>> GetFloorsByBlockAsync(int blockId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<PagedResultDto<RoomDto>> GetRoomsAsync(FilterRoomDto filter, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddRoomImagesAsync(int roomId, string[] imageUrls, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task RemoveRoomImageAsync(int imageId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<List<AmenityDto>> GetAmenitiesAsync(CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task SetRoomAmenitiesAsync(int roomId, int[] amenityIds, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddRoomPriceHistoryAsync(int roomId, RoomPriceHistoryDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<List<RoomPriceHistoryDto>> GetRoomPriceHistoryAsync(int roomId, CancellationToken ct = default)
        => throw new NotImplementedException();

    // ================== MAPPER ==================

    private static RoomDto MapToDto(Room r) => new()
    {
        RoomId = r.RoomId,
        FloorId = r.FloorId,
        RoomCode = r.RoomCode,
        RoomName = r.RoomName,
        AreaM2 = r.AreaM2,
        MaxOccupants = r.MaxOccupants,
        Status = r.Status,
        CurrentBasePrice = r.CurrentBasePrice,
        Description = r.Description
    };
}