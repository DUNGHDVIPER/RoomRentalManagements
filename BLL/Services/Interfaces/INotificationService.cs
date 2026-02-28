using BLL.Common;
using BLL.DTOs.Common;
using BLL.DTOs.Notification;

namespace BLL.Services.Interfaces;

public interface INotificationService
{
  
        Task BroadcastAsync(BroadcastNotificationDto dto, CancellationToken ct = default);

        Task<PagedResultDto<NotificationDto>> GetUserNotificationsAsync(
            int tenantId,
            PagedRequestDto request,
            CancellationToken ct = default);

        Task MarkReadAsync(int notificationId, CancellationToken ct = default);

        Task<List<NotificationDto>> GetHostHistoryAsync(CancellationToken ct = default);
    }

