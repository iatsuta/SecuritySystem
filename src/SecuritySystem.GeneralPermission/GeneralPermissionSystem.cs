using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;
using SecuritySystem.GeneralPermission.AvailableSecurityRoleSource;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionRestrictionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
    TSecurityContextObjectIdent, TSecurityContextTypeIdent, TSecurityContext, TSecurityContextIdent>(
    IServiceProvider serviceProvider,
    IQueryableSource queryableSource,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityContextTypeIdentConverter,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
    SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo

    )
    : IPermissionRestrictionSource<TPermission, TSecurityContextIdent>
    where TPrincipal : class
    where TPermission : class
    where TSecurityRole : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
    where TSecurityContextTypeIdent : notnull

    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr()
    {
        var securityContextIdentConverter = serviceProvider.GetRequiredService<ISecurityIdentityConverter<TSecurityContextIdent>>();

        var convertToTargetIdentExpr = securityContextIdentConverter.GetConvertExpression<TSecurityContextObjectIdent>();

        var securityContextTypeId = securityContextTypeIdentConverter.Convert(securityContextInfoSource.GetSecurityContextInfo<TSecurityContext>().Identity).Id;

        var restrictionQueryable = queryableSource.GetQueryable<TPermissionRestriction>();

        var isSecurityContextTypeExpr = ExpressionHelper.GetEqualityWithExpr(securityContextTypeId);

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TPermission, IEnumerable<TSecurityContextIdent>>>(ee =>
        {
            if (restrictionFilterInfo == null)
            {
                return permission => restrictionQueryable
                    .Where(restriction => ee.Evaluate(bindingInfo.Permission.Path, restriction) == permission)
                    .Where(restriction => ee.Evaluate(isSecurityContextTypeExpr, ee.Evaluate(securityContextTypeIdentityInfo.Id.Path, ee.Evaluate(bindingInfo.SecurityContextType.Path, restriction))))
                    .Select(restriction => ee.Evaluate(convertToTargetIdentExpr, ee.Evaluate(bindingInfo.SecurityContextObjectId.Path, restriction)));
            }
            else
            {
                var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>();

                var securityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo)
                    .Where(restrictionFilterInfo.GetPureFilter(serviceProvider))
                    .Select(identityInfo.Id.Path);

                throw new NotImplementedException();
                //return permission => permission.Restrictions
                //	.Where(restriction => restriction.SecurityContextType.Id == securityContextTypeId)
                //	.Where(restriction => securityContextQueryable.Contains(restriction.SecurityContextId))
                //	.Select(restriction => restriction.SecurityContextId);
            }
        });
    }
}

public class GeneralPermissionSystem<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
(
	IServiceProvider serviceProvider,
	GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
	SecurityRuleCredential securityRuleCredential)
	: IPermissionSystem<TPermission>

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
{
	public Type PermissionType { get; } = typeof(TPermission);


    public IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext where TSecurityContextIdent : notnull
    {
        throw new NotImplementedException();
    }

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return ActivatorUtilities
            .CreateInstance<GeneralPermissionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                TSecurityContextObjectIdent>>(
                serviceProvider,
                securityRule.TryApplyCredential(securityRuleCredential));
    }

	public Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken)
	{
		return ActivatorUtilities.CreateInstance<GeneralAvailableSecurityRoleSource<TPrincipal, TPermission, TSecurityRole>>(serviceProvider, securityRuleCredential)
								 .GetAvailableSecurityRoles(cancellationToken);
	}

	IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
	{
		return this.GetPermissionSource(securityRule);
	}
}
