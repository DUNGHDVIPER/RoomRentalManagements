using BLL.DTOs.Notification;
using BLL.DTOs.Common;
using BLL.Common;
using BLL.Services.Interfaces;

namespace BLL.Services;

public class NotificationService : INotificationService
{
    public Task BroadcastAsync(BroadcastNotificationDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<PagedResultDto<NotificationDto>> GetUserNotificationsAsync(string userId, PagedRequestDto req, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task MarkReadAsync(long notificationId, CancellationToken ct = default)
        => throw new NotImplementedException();
}
