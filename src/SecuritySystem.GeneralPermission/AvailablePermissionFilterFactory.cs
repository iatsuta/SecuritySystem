using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.VisualIdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class AvailablePermissionFilterFactory<TPermission>(IServiceProvider serviceProvider, GeneralPermissionBindingInfo bindingInfo) : IAvailablePermissionFilterFactory<TPermission>
{
    private readonly Lazy<IAvailablePermissionFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var innerServiceType = typeof(AvailablePermissionFilterFactory<,>).MakeGenericType(bindingInfo.PrincipalType, typeof(TPermission));

        return (IAvailablePermissionFilterFactory<TPermission>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo);
    });

    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.lazyInnerService.Value.CreateFilter(securityRule);
}

public class AvailablePermissionFilterFactory<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    GeneralPermissionBindingInfo<TPrincipal, TPermission> bindingInfo,
    TimeProvider timeProvider,
    IUserNameResolver<TPrincipal> userNameResolver,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver,
    IPermissionSecurityRoleFilterFactory<TPermission> permissionSecurityRoleFilterFactory,
    SecurityRuleCredential defaultSecurityRuleCredential) : IAvailablePermissionFilterFactory<TPermission>
{
    private readonly VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>();

    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.GetFilterElements(securityRule).BuildAnd();

    private IEnumerable<Expression<Func<TPermission, bool>>> GetFilterElements(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (bindingInfo.Period != null)
        {
            var today = timeProvider.GetUtcNow().Date;

            yield return

                from period in bindingInfo.Period.Path

                select period.StartDate <= today && (period.EndDate == null || today <= period.EndDate);
        }

        var principalName = userNameResolver.Resolve(securityRule.CustomCredential ?? defaultSecurityRuleCredential);

        if (principalName != null)
        {
            yield return bindingInfo.Principal.Path.Select(principalVisualIdentityInfo.Name.Path).Select(name => name == principalName);
        }

        foreach (var (securityRoleIdentType, securityRoleIdents) in securityRolesIdentsResolver.Resolve(securityRule))
        {
            yield return permissionSecurityRoleFilterFactory.CreateFilter(securityRoleIdentType, securityRoleIdents);
        }

        foreach (var securityContextRestriction in securityRule.GetSafeSecurityContextRestrictions())
        {
            if (securityContextRestriction.Required)
            {

            }

            if (securityContextRestriction.RawFilter != null)
            {
                var basePureFilter = securityContextRestriction.RawFilter.GetBasePureFilter(serviceProvider);

                IPermissionRestrictionFilterFactory<>

                IPermissionRestrictionFilterFactory<>
                var filter = this.GetRestrictionFilter(securityContextRestriction.RawFilter!)

            }
            var baseFilter =
                ExpressionEvaluateHelper.InlineEvaluate(ee =>
                                                            ExpressionHelper
                                                                .Create((TPermission permission) =>
                                                                            permission.Restrictions.Any(r => r.SecurityContextType.Id
                                                                                        == securityContextTypeId
                                                                                        && ee.Evaluate(
                                                                                            restrictionFilterExpr,
                                                                                            r.SecurityContextId))));

            if (allowGrandAccess)
            {
                var grandAccessExpr = ExpressionHelper.Create(
                    (TPermission permission) =>
                        permission.Restrictions.All(r => r.SecurityContextType.Id != securityContextTypeId));

                yield return baseFilter.BuildOr(grandAccessExpr);
            }
            else
            {
                yield return baseFilter;
            }
        }
    }


    //public AvailablePermissionFilter<TSecurityContextObjectIdent> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    //{
    //    var restrictionFiltersRequest =

    //        from securityContextRestriction in securityRule.GetSafeSecurityContextRestrictions()

    //        where securityContextRestriction.RawFilter != null

    //        let filter = this.GetRestrictionFilter(securityContextRestriction.RawFilter!)

    //        let securityContextType = securityContextInfoSource.GetSecurityContextInfo(securityContextRestriction.SecurityContextType)

    //        select (securityIdentityConverter.Convert(securityContextType.Identity).Id, (!securityContextRestriction.Required, filter));


    //    return new AvailablePermissionFilter<TSecurityContextObjectIdent>()
    //    {
    //        Date = timeProvider.GetUtcNow().Date,
    //        PrincipalName = userNameResolver.Resolve(securityRule.CustomCredential ?? defaultSecurityRuleCredential),
    //        SecurityRoleIdents = securityRolesIdentsResolver.Resolve(securityRule),
    //        RestrictionFilters = restrictionFiltersRequest.ToDictionary()
    //    };
    //}

    private Expression<Func<TSecurityContextObjectIdent, bool>> GetRestrictionFilter(SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<SecurityContextRestrictionFilterInfo<ISecurityContext>, Expression<Func<TSecurityContextObjectIdent, bool>>>(this.GetRestrictionFilterExpression)
               .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
               .Invoke<Expression<Func<TSecurityContextObjectIdent, bool>>>(this, restrictionFilterInfo);
    }

    private Expression<Func<TSecurityContextObjectIdent, bool>> GetRestrictionFilterExpression<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextObjectIdent>();

        var filteredSecurityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo)
                                                                    .Select(identityInfo.Id.Path);

        return securityContextId => filteredSecurityContextQueryable.Contains(securityContextId);
    }
}