using System.Linq.Expressions;

using CommonFramework.IdentitySource;

using HierarchicalExpand;

namespace SecuritySystem.Notification;

public class HierarchicalDirectLevelExtractor<TSecurityContext, TSecurityContextIdent>(
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo,
    DeepLevelInfo<TSecurityContext> deepLevelInfo)
    : DirectLevelExtractor<TSecurityContext, TSecurityContextIdent>
    where TSecurityContextIdent : notnull
{
    protected override Expression<Func<IQueryable<TSecurityContext>, int>> GetDirectLevelExpression(
        NotificationFilterGroup<TSecurityContextIdent> notificationFilterGroup)
    {
        var expandedSecIdents = notificationFilterGroup.ExpandType.IsHierarchical()
            ? hierarchicalObjectExpanderFactory.Create<TSecurityContextIdent>(notificationFilterGroup.SecurityContextType).Expand(
                notificationFilterGroup.Idents,
                HierarchicalExpandType.Parents)
            : notificationFilterGroup.Idents;

        var containsFilter = identityInfo.CreateContainsFilter(expandedSecIdents);

        return permissionSecurityContextItems => permissionSecurityContextItems
                                                     .Where(containsFilter)
                                                     .Select(deepLevelInfo.DeepLevel.Path)
                                                     .Select(v => (int?)v)
                                                     .Max()
                                                 ?? PriorityLevels.AccessDenied;
    }
}