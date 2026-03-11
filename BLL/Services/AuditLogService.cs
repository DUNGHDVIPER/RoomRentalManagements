using BLL.Common;
using BLL.DTOs.Common;
using BLL.Services.Interfaces;

namespace BLL.Services;

public class AuditLogService : IAuditLogService
{
    public Task WriteAsync(BLL.DTOs.Audit.AuditLogDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<PagedResultDto<DTOs.Audit.AuditLogDto>> QueryAsync(
        BLL.DTOs.Audit.AuditLogQueryDto query, CancellationToken ct = default)
        => throw new NotImplementedException();
}
