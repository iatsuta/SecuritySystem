using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.Services;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class RawPermissionConverter<TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource bindingInfoSource)
    : IRawPermissionConverter<TPermissionRestriction>
    where TPermissionRestriction : class
{
    private readonly Lazy<IRawPermissionConverter<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var securityContextTypeIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityContextTypeType);

        var innerServiceType = typeof(RawPermissionConverter<,,,>).MakeGenericType(
            bindingInfo.PermissionRestrictionType,
            bindingInfo.SecurityContextTypeType,
            bindingInfo.SecurityContextObjectIdentType,
            securityContextTypeIdentityInfo.IdentityType);

        return (IRawPermissionConverter<TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            securityContextTypeIdentityInfo);
    });

    public Dictionary<Type, Array> ConvertPermission(
        DomainSecurityRule.RoleBaseSecurityRule securityRule,
        IReadOnlyList<TPermissionRestriction> restrictions,
        IEnumerable<Type> securityContextTypes) => lazyInnerService.Value.ConvertPermission(securityRule, restrictions, securityContextTypes);
}

public class RawPermissionConverter<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityContextTypeIdent>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityIdentityConverter,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo) : IRawPermissionConverter<TPermissionRestriction>

    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
    where TSecurityContextTypeIdent : notnull

{
    public Dictionary<Type, Array> ConvertPermission(DomainSecurityRule.RoleBaseSecurityRule securityRule, IReadOnlyList<TPermissionRestriction> restrictions,
        IEnumerable<Type> securityContextTypes)
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

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter(List<TSecurityContextObjectIdent> securityContextIdents,
        SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<List<TSecurityContextObjectIdent>, SecurityContextRestrictionFilterInfo<ISecurityContext>, TSecurityContextObjectIdent[]>(
                this.ApplySecurityContextFilter)
            .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
            .Invoke<TSecurityContextObjectIdent[]>(this, securityContextIdents, restrictionFilterInfo);
    }

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter<TSecurityContext>(List<TSecurityContextObjectIdent> baseSecurityContextIdents,
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