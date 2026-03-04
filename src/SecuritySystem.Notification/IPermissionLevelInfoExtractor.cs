using System.Linq.Expressions;

namespace SecuritySystem.Notification;

public interface IPermissionLevelInfoExtractor<TPermission>
{
    Expression<Func<PermissionLevelInfo<TPermission>, FullPermissionLevelInfo<TPermission>>> GetSelector(NotificationFilterGroup notificationFilterGroup);
}
