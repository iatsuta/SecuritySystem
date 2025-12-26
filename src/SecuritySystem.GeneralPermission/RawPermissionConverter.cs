using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.IdentitySource;

namespace SecuritySystem.GeneralPermission;

public class RawPermissionConverter<TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IRawPermissionConverter<TPermissionRestriction>
    where TPermissionRestriction : class
{
    private readonly Lazy<IRawPermissionConverter<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(RawPermissionConverter<,>).MakeGenericType(
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IRawPermissionConverter<TPermissionRestriction>>(innerServiceType);
    });

    public Dictionary<Type, Array> ConvertPermission(
        DomainSecurityRule.RoleBaseSecurityRule securityRule,
        IReadOnlyList<TPermissionRestriction> restrictions,
        IEnumerable<Type> securityContextTypes) => lazyInnerService.Value.ConvertPermission(securityRule, restrictions, securityContextTypes);
}

public class RawPermissionConverter<TPermissionRestriction, TSecurityContextObjectIdent>(
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    IPermissionRestrictionRawConverter<TPermissionRestriction> permissionRestrictionRawConverter) : IRawPermissionConverter<TPermissionRestriction>

    where TPermissionRestriction : class
    where TSecurityContextObjectIdent : notnull
{
    public Dictionary<Type, Array> ConvertPermission(DomainSecurityRule.RoleBaseSecurityRule securityRule, IReadOnlyList<TPermissionRestriction> restrictions,
        IEnumerable<Type> securityContextTypes)
    {
        var rawRestrictions = permissionRestrictionRawConverter.Convert(restrictions);

        var filterInfoDict = securityRule.GetSafeSecurityContextRestrictionFilters().ToDictionary(filterInfo => filterInfo.SecurityContextType);

        return securityContextTypes.ToDictionary(
            securityContextType => securityContextType,
            securityContextType =>
            {
                var securityContextRestrictionFilterInfo = filterInfoDict.GetValueOrDefault(securityContextType);

                var baseIdents = rawRestrictions.GetValueOrDefault(securityContextType, Array.Empty<TSecurityContextObjectIdent>());

                if (securityContextRestrictionFilterInfo == null)
                {
                    return baseIdents;
                }
                else
                {
                    return this.ApplySecurityContextFilter(baseIdents, securityContextRestrictionFilterInfo);
                }

            });
    }

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter(Array securityContextIdents, SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<TSecurityContextObjectIdent[], SecurityContextRestrictionFilterInfo<ISecurityContext>, TSecurityContextObjectIdent[]>(
                this.ApplySecurityContextFilter)
            .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
            .Invoke<TSecurityContextObjectIdent[]>(this, securityContextIdents, restrictionFilterInfo);
    }

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter<TSecurityContext>(TSecurityContextObjectIdent[] baseSecurityContextIdents,
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextObjectIdent>();

        var filteredSecurityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo).Select(identityInfo.Id.Path);

        if (baseSecurityContextIdents.Any())
        {
            return filteredSecurityContextQueryable.Where(securityContextId => baseSecurityContextIdents.Contains(securityContextId))
                .ToArray();
        }
        else
        {
            return filteredSecurityContextQueryable.ToArray();
        }
    }
}