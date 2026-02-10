namespace BLL.DTOs.Notification;

public class BroadcastNotificationDto
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string[]? TargetRoles { get; set; } // null = all
}
