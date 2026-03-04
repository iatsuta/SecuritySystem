using System.Linq.Expressions;

namespace SecuritySystem.Notification;

public interface INotificationGeneralPermissionFilterFactory<TPermission>
{
    Expression<Func<TPermission, bool>> Create(IEnumerable<SecurityRole> securityRoles);
}
