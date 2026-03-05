using System.Linq.Expressions;

namespace SecuritySystem.Notification;

public interface IDirectLevelExtractor<TSecurityContext>
{
    Expression<Func<IQueryable<TSecurityContext>, int>> GetDirectLevelExpression(NotificationFilterGroup notificationFilterGroup);
}