using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

namespace SecuritySystem.Notification;

public abstract class PermissionLevelInfoExtractor<TPermission> : IPermissionLevelInfoExtractor<TPermission>
{
    public Expression<Func<PermissionLevelInfo<TPermission>, FullPermissionLevelInfo<TPermission>>> GetSelector(NotificationFilterGroup notificationFilterGroup)
    {
        var genericExtractorType = hierarchicalInfoSource.IsHierarchical(securityContextType)
            ? typeof(PermissionLevelInfoExtractor<,>)
            : typeof(PermissionLevelInfoPlainExtractor<,>);

        var extractorType = genericExtractorType.MakeGenericType(securityContextType);

        return serviceProxyFactory.Create<IPermissionLevelInfoExtractor<TPermission>>(extractorType);
    }
}


public class PermissionLevelInfoExtractor<TPermission, TSecurityContext, TSecurityContextIdent>(
    IQueryableSource queryableSource,
    IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo,
    IDirectLevelExtractor<TSecurityContext> directLevelExtractor) : IPermissionLevelInfoExtractor<TPermission>
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    public Expression<Func<PermissionLevelInfo<TPermission>, FullPermissionLevelInfo<TPermission>>> GetSelector(NotificationFilterGroup notificationFilterGroup)
    {
        var grandAccess = notificationFilterGroup.ExpandType.AllowEmpty();

        var securityContextQ = queryableSource.GetQueryable<TSecurityContext>();

        var getDirectLevelExpression = directLevelExtractor.GetDirectLevelExpression(notificationFilterGroup);

        return ExpressionEvaluateHelper
            .InlineEvaluate(ee =>
            {
                return from permissionInfo in ExpressionHelper.GetIdentity<PermissionLevelInfo<TPermission>>()

                       let permission = permissionInfo.Permission

                       let permissionSecurityContextItems =
                           securityContextQ.Where(securityContext => permission.Restrictions
                                                                               .Any(fi => fi.SecurityContextType.Name
                                                                                          == typeof(TSecurityContext).Name
                                                                                          && fi.SecurityContextId
                                                                                          == ee.Evaluate(
                                                                                              identityInfo.Id.Path,
                                                                                              securityContext)))


                       let directLevel = ee.Evaluate(getDirectLevelExpression, permissionSecurityContextItems)

                       let grandLevel =
                           grandAccess
                           && permission.Restrictions.All(fi => fi.SecurityContextType.Name
                                                                != typeof(TSecurityContext).Name)
                               ? PriorityLevels.GrandAccess
                               : PriorityLevels.AccessDenied

                       let level = Math.Max(directLevel, grandLevel)

                       select new FullPermissionLevelInfo<TPermission> { Permission = permissionInfo.Permission, LevelInfo = permissionInfo.LevelInfo, Level = level };
            });
    }
}
