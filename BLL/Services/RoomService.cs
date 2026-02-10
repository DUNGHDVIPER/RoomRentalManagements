using BLL.ApiClients;
using BLL.Common;
using BLL.Dtos;
using BLL.DTOs.Room;
using BLL.Services.Interfaces;

namespace BLL.Services;

public class RoomService : IRoomService
{
    private readonly MockApiClient _api;

    public RoomService(MockApiClient api)
    {
        _api = api;
    }

    public Task AddRoomImagesAsync(int roomId, string[] imageUrls, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task AddRoomPriceHistoryAsync(int roomId, RoomPriceHistoryDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<BlockDto> CreateBlockAsync(BlockDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteBlockAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRoomAsync(int roomId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rooms = await _api.GetAsync<List<RoomDto>>("/rooms", ct);
        return rooms ?? [];
    }

    public Task<string?> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<AmenityDto>> GetAmenitiesAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<BlockDto>> GetBlocksAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _api.GetAsync<RoomDto>($"/rooms/{id}", ct);
    }

    public Task<List<FloorDto>> GetFloorsByBlockAsync(int blockId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<RoomDto> GetRoomDetailAsync(int roomId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<RoomPriceHistoryDto>> GetRoomPriceHistoryAsync(int roomId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResultDto<RoomDto>> GetRoomsAsync(FilterRoomDto filter, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveRoomImageAsync(int imageId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SetRoomAmenitiesAsync(int roomId, int[] amenityIds, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<BlockDto> UpdateBlockAsync(int id, BlockDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<RoomDto> UpdateRoomAsync(int roomId, UpdateRoomDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
