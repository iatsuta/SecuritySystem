namespace SecuritySystem.Notification;

public interface INotificationPermissionExtractor<out TPermission>
{
    IAsyncEnumerable<TPermission> GetPermissionsAsync(IEnumerable<SecurityRole> securityRoles, IEnumerable<NotificationFilterGroup> notificationFilterGroups);
}