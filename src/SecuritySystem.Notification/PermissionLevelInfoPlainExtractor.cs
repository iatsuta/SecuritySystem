using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

using System.Linq.Expressions;

namespace SecuritySystem.Notification;

public class PermissionLevelInfoPlainExtractor<TPermission, TSecurityContext, TSecurityContextIdent>(
    IQueryableSource queryableSource,
    IIdentityInfoSource identityInfoSource)
    : PermissionLevelInfoExtractor<TPermission, TSecurityContext, TSecurityContextIdent>(queryableSource, identityInfoSource)
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    protected override Expression<Func<IQueryable<TSecurityContext>, int>> GetDirectLevelExpression(
        NotificationFilterGroup<TSecurityContextIdent> notificationFilterGroup)
    {
        var containsFilter = this.IdentityInfo.CreateContainsFilter(notificationFilterGroup.Idents);

        return permissionSecurityContextItems => permissionSecurityContextItems.Any(containsFilter) ? 0 : PriorityLevels.AccessDenied;
    }
}