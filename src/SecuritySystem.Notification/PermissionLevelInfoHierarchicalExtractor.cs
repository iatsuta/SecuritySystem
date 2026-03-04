using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

using HierarchicalExpand;

using System.Linq.Expressions;

namespace SecuritySystem.Notification;

public class PermissionLevelInfoHierarchicalExtractor<TPermission, TSecurityContext, TSecurityContextIdent>(
    IQueryableSource queryableSource,
    IIdentityInfoSource identityInfoSource,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    DeepLevelInfo<TSecurityContext> deepLevelInfo) : PermissionLevelInfoExtractor<TPermission, TSecurityContext, TSecurityContextIdent>(queryableSource, identityInfoSource)
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    protected override Expression<Func<IQueryable<TSecurityContext>, int>> GetDirectLevelExpression(NotificationFilterGroup<TSecurityContextIdent> notificationFilterGroup)
    {
        var expandedSecIdents = notificationFilterGroup.ExpandType.IsHierarchical()
                                    ? hierarchicalObjectExpanderFactory.Create<TSecurityContextIdent>(notificationFilterGroup.SecurityContextType).Expand(
                                        notificationFilterGroup.Idents,
                                        HierarchicalExpandType.Parents)
                                    : notificationFilterGroup.Idents;

        var containsFilter = this.IdentityInfo.CreateContainsFilter(expandedSecIdents);

        return permissionSecurityContextItems => permissionSecurityContextItems
                                                 .Where(containsFilter)
                                                 .Select(deepLevelInfo.DeepLevel.Path)
                                                 .Select(v => (int?)v)
                                                 .Max()
                                                 ?? PriorityLevels.AccessDenied;
    }
}
