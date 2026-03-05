namespace SecuritySystem.Notification;

public class NotificationPrincipalExtractor<TPrincipal, TPermission>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    INotificationPermissionExtractor<TPermission> notificationPermissionExtractor)
    : INotificationPrincipalExtractor<TPrincipal>
{
    public IAsyncEnumerable<TPrincipal> GetPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, IEnumerable<NotificationFilterGroup> notificationFilterGroups)
    {
        return notificationPermissionExtractor
            .GetPermissionsAsync(securityRoles, notificationFilterGroups)
            .Select(bindingInfo.Principal.Getter)
            .Distinct();
    }
}