using CommonFramework;
using CommonFramework.IdentitySource;
using HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class RawPermissionConverter<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityContextTypeIdent>(
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    ISecurityContextInfoSource securityContextInfoSource,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo,
    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityIdentityConverter) : IRawPermissionConverter<TPermissionRestriction>


    where TPrincipal : class
        where TPermission : class
        where TSecurityRole : class
        where TPermissionRestriction : class
        where TSecurityContextType : class
        where TSecurityContextObjectIdent : notnull
    where TSecurityContextTypeIdent : notnull

{
    public Dictionary<Type, Array> ConvertPermission(DomainSecurityRule.RoleBaseSecurityRule securityRule, IReadOnlyList<TPermissionRestriction> restrictions, IEnumerable<Type> securityContextTypes)
    {
        var purePermission = restrictions.GroupBy(
                bindingInfo.SecurityContextType.Getter.Composite(securityContextTypeIdentityInfo.Id.Getter),
                bindingInfo.SecurityContextObjectId.Getter)

            .ToDictionary(g => g.Key, g => g.ToList());

        var filterInfoDict = securityRule.GetSafeSecurityContextRestrictionFilters().ToDictionary(filterInfo => filterInfo.SecurityContextType);

        return securityContextTypes.ToDictionary(
            securityContextType => securityContextType,
            Array (securityContextType) =>
            {
                var securityContextRestrictionFilterInfo = filterInfoDict.GetValueOrDefault(securityContextType);

                var securityContextTypeIdentity = securityContextInfoSource.GetSecurityContextInfo(securityContextType).Identity;

                var securityContextTypeId = securityIdentityConverter.Convert(securityContextTypeIdentity).Id;

                var baseIdents = purePermission.GetValueOrDefault(securityContextTypeId, []);

                if (securityContextRestrictionFilterInfo == null)
                {
                    return baseIdents.ToArray();
                }
                else
                {
                    return this.ApplySecurityContextFilter(baseIdents, securityContextRestrictionFilterInfo);
                }

            });
    }

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter(List<TSecurityContextObjectIdent> securityContextIdents, SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<List<TSecurityContextObjectIdent>, SecurityContextRestrictionFilterInfo<ISecurityContext>, TSecurityContextObjectIdent[]>(this.ApplySecurityContextFilter)
            .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
            .Invoke<TSecurityContextObjectIdent[]>(this, securityContextIdents, restrictionFilterInfo);
    }

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter<TSecurityContext>(List<TSecurityContextObjectIdent> baseSecurityContextIdents, SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
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