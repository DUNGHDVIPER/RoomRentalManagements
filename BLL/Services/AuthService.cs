using System.Text.Json;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Entities.Motel;

namespace BLL.Services;

public class AuditService : IAuditService
{
    private readonly MotelManagementDbContext _db;

    public AuditService(MotelManagementDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        int? actorUserId,
        string action,
        string entityType,
        string entityId,
        string? note = null,
        object? oldValue = null,
        object? newValue = null,
        CancellationToken ct = default)
    {
        var opt = new JsonSerializerOptions { WriteIndented = false };

        string? oldJson = oldValue == null ? null : JsonSerializer.Serialize(oldValue, opt);
        string? newJson = newValue == null ? null : JsonSerializer.Serialize(newValue, opt);

        _db.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Note = note,
            OldValueJson = oldJson,
            NewValueJson = newJson,
            CreatedAt = DateTime.UtcNow
        });

        // IMPORTANT: dùng chung DbContext => chạy cùng transaction nếu caller đang BeginTransaction
        await _db.SaveChangesAsync(ct);
    }
}