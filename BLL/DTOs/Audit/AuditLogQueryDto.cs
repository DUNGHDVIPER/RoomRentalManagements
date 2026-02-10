using BLL.DTOs.Common;

namespace BLL.DTOs.Audit;

public class AuditLogQueryDto : PagedRequestDto
{
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
