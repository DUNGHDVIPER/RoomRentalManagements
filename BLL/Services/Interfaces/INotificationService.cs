using BLL.Common;
using BLL.DTOs.Common;
using BLL.DTOs.Notification;

namespace BLL.Services.Interfaces;

public interface INotificationService
{
    Task BroadcastAsync(BroadcastNotificationDto dto, CancellationToken ct = default);
    Task<PagedResultDto<NotificationDto>> GetUserNotificationsAsync(string userId, PagedRequestDto req, CancellationToken ct = default);
    Task MarkReadAsync(long notificationId, CancellationToken ct = default);
}
