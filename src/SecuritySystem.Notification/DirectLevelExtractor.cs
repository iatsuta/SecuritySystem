using System.Linq.Expressions;

namespace SecuritySystem.Notification;

public abstract class DirectLevelExtractor<TSecurityContext, TSecurityContextIdent> : IDirectLevelExtractor<TSecurityContext>
{
    public Expression<Func<IQueryable<TSecurityContext>, int>> GetDirectLevelExpression(NotificationFilterGroup notificationFilterGroup) =>
        this.GetDirectLevelExpression((NotificationFilterGroup<TSecurityContextIdent>)notificationFilterGroup);

    protected abstract Expression<Func<IQueryable<TSecurityContext>, int>> GetDirectLevelExpression(
        NotificationFilterGroup<TSecurityContextIdent> notificationFilterGroup);
}