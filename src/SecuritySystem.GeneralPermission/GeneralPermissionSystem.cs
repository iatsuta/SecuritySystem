using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSystem<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityContextTypeIdent>
(
	IServiceProvider serviceProvider,
	GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> info,
	IQueryableSource queryableSource,
	ISecurityContextInfoSource securityContextInfoSource,
	ISecurityIdentityConverter<TSecurityContextTypeIdent> securityContextTypeIdentConverter,
	IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo,
	ISecurityContextSource securityContextSource,
	IIdentityInfoSource identityInfoSource,
	SecurityRuleCredential securityRuleCredential)
	: IPermissionSystem<TPermission>

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
	where TSecurityContextTypeIdent : notnull
{
	public Type PermissionType { get; } = typeof(TPermission);

	public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetPermissionRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(
		SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
		where TSecurityContext : class, ISecurityContext
		where TSecurityContextIdent : notnull
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
					.Where(restriction => ee.Evaluate(info.ToPermission.Path, restriction) == permission)
					.Where(restriction => ee.Evaluate(isSecurityContextTypeExpr, ee.Evaluate(securityContextTypeIdentityInfo.Id.Path, ee.Evaluate(info.ToSecurityContextType.Path, restriction))))
					.Select(restriction => ee.Evaluate(convertToTargetIdentExpr, ee.Evaluate(info.ToSecurityContextObjectId.Path, restriction)));
			}
			else
			{
				var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>();

				var securityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo)
					.Where(restrictionFilterInfo.GetPureFilter(serviceProvider))
					.Select(identityInfo.Id.Path);

				return permission => permission.Restrictions
					.Where(restriction => restriction.SecurityContextType.Id == securityContextTypeId)
					.Where(restriction => securityContextQueryable.Contains(restriction.SecurityContextId))
					.Select(restriction => restriction.SecurityContextId);
			}
		});
	}

	public Expression<Func<TPermission, bool>> GetGrandAccessExpr<TSecurityContext, TSecurityContextIdent>()
		where TSecurityContext : class, ISecurityContext
		where TSecurityContextIdent : notnull =>
		this.GetPermissionRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(null).Select(v => !v.Any());

	public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
	{
		return ActivatorUtilities.CreateInstance<GeneralPermissionSource>(
			serviceProvider,
			securityRule.TryApplyCredential(securityRuleCredential));
	}

	public Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken)
	{
		return ActivatorUtilities.CreateInstance<GeneralAvailableSecurityRoleSource>(serviceProvider, securityRuleCredential)
								 .GetAvailableSecurityRoles(cancellationToken);
	}

	IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
	{
		return this.GetPermissionSource(securityRule);
	}
}
