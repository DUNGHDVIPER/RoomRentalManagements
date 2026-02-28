using BLL.Common;
using BLL.DTOs.Common;
using BLL.DTOs.Notification;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Entities.System;
using Microsoft.EntityFrameworkCore;

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