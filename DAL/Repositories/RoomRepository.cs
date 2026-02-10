using DAL.Data;
using DAL.Entities.Property;
using DAL.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations;

public sealed class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;

    public RoomRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Room?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);


    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.Rooms.AsNoTracking()
            .OrderBy(r => r.RoomNo)
            .ToListAsync(ct);

        return list;
    }

    public async Task AddAsync(Room entity, CancellationToken ct = default)
    {
        await _db.Rooms.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Room entity, CancellationToken ct = default)
    {
        _db.Rooms.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Rooms.FindAsync([id], ct);
        if (entity is null) return;

        _db.Rooms.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
