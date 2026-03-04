namespace SecuritySystem.Notification;

public interface INotificationPrincipalExtractor<TPrincipal>
{
    Task<List<TPrincipal>> GetPrincipalsAsync(SecurityRole[] securityRoles, IEnumerable<NotificationFilterGroup> notificationFilterGroups, CancellationToken cancellationToken = default);
}
