namespace SecuritySystem.Notification;

public interface INotificationPrincipalExtractor<out TPrincipal>
{
    IAsyncEnumerable<TPrincipal> GetPrincipalsAsync(SecurityRole[] securityRoles, IEnumerable<NotificationFilterGroup> notificationFilterGroups);
}
