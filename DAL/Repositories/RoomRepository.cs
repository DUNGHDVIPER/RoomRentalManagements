using DAL.Data;
using DAL.Entities.Motel;
using DAL.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public sealed class RoomRepository : IRoomRepository

{
    private readonly MotelManagementDbContext _db;
    public RoomRepository(MotelManagementDbContext db) => _db = db;

    public Task<Room?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomId == id, ct);

    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken ct = default)
        => await _db.Rooms.AsNoTracking().OrderBy(r => r.RoomCode).ToListAsync(ct);

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Room entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Room entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
