namespace SecuritySystem.Notification;

public interface INotificationPermissionExtractor<TPermission>
{
    Task<List<TPermission>> GetPermissionsAsync(SecurityRole[] securityRoles, IEnumerable<NotificationFilterGroup> notificationFilterGroups, CancellationToken cancellationToken = default);
}
