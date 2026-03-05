namespace SecuritySystem.Notification;

public interface INotificationPrincipalExtractor<out TPrincipal>
{
    IAsyncEnumerable<TPrincipal> GetPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, IEnumerable<NotificationFilterGroup> notificationFilterGroups);
}
