using BLL.Common;
using BLL.DTOs.Audit;


namespace BLL.Services.Interfaces;

public interface IAuditLogService
{
    Task WriteAsync(AuditLogDto dto, CancellationToken ct = default);
    Task<PagedResultDto<AuditLogDto>> QueryAsync(AuditLogQueryDto query, CancellationToken ct = default);
}
