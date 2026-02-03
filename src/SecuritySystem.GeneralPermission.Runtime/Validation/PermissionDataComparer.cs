using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class PermissionEqualityComparer<TPermission, TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionBindingInfoSource generalBindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource,
    IIdentityInfoSource identityInfoSource) : IPermissionEqualityComparer<TPermission, TPermissionRestriction>
{
    private readonly Lazy<IPermissionEqualityComparer<TPermission, TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var generalBindingInfo = generalBindingInfoSource.GetForPermission(typeof(TPermission));

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(typeof(TPermission));

        var securityContextTypeIdentityInfo = identityInfoSource.GetIdentityInfo(restrictionBindingInfo.SecurityContextTypeType);

        var innerServiceType = typeof(PermissionEqualityComparer<,,,,,>)
            .MakeGenericType(
                generalBindingInfo.PermissionType,
                generalBindingInfo.SecurityRoleType,
                restrictionBindingInfo.PermissionRestrictionType,
                restrictionBindingInfo.SecurityContextTypeType,
                restrictionBindingInfo.SecurityContextObjectIdentType,
                securityContextTypeIdentityInfo.IdentityType);

        return serviceProxyFactory.Create<IPermissionEqualityComparer<TPermission, TPermissionRestriction>>(
            innerServiceType,
            bindingInfo,
            generalBindingInfo,
            restrictionBindingInfo,
            securityContextTypeIdentityInfo);
    });

    public bool Equals(PermissionData<TPermission, TPermissionRestriction>? x, PermissionData<TPermission, TPermissionRestriction>? y) =>
        this.lazyInnerService.Value.Equals(x, y);

    public int GetHashCode(PermissionData<TPermission, TPermissionRestriction> obj) =>
        this.lazyInnerService.Value.GetHashCode(obj);
}

public class PermissionEqualityComparer<TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityContextTypeIdent>(
    PermissionBindingInfo<TPermission> bindingInfo,
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> restrictionBindingInfo,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo)
    : IPermissionEqualityComparer<TPermission, TPermissionRestriction>

    where TPermission : class
    where TSecurityRole : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
    where TSecurityContextTypeIdent : notnull
{
    protected virtual IEqualityComparer<IGrouping<TSecurityContextType, TSecurityContextObjectIdent>> RestrictionGroupComparer { get; } =

        new EqualityComparerImpl<IGrouping<TSecurityContextType, TSecurityContextObjectIdent>>(
            (g1, g2) => g1.Key == g2.Key && g1.SequenceEqual(g2),
            g => g.Key.GetHashCode());

    public bool Equals(PermissionData<TPermission, TPermissionRestriction>? permissionData,
        PermissionData<TPermission, TPermissionRestriction>? otherPermissionData)
    {
        if (object.ReferenceEquals(permissionData, otherPermissionData))
        {
            return true;
        }

        if (permissionData is null || otherPermissionData is null)
        {
            return false;
        }

        return this.PureEquals(permissionData, otherPermissionData);
    }

    protected virtual bool PureEquals(
        PermissionData<TPermission, TPermissionRestriction> permissionData,
        PermissionData<TPermission, TPermissionRestriction> otherPermissionData)
    {
        return generalBindingInfo.SecurityRole.Getter(permissionData.Permission) == generalBindingInfo.SecurityRole.Getter(otherPermissionData.Permission)
               && bindingInfo.GetSafePeriod(permissionData.Permission).IsIntersected(bindingInfo.GetSafePeriod(otherPermissionData.Permission))
               && this.EqualsRestrictions(permissionData, otherPermissionData);
    }

    protected virtual bool EqualsRestrictions(
        PermissionData<TPermission, TPermissionRestriction> permissionData,
        PermissionData<TPermission, TPermissionRestriction> otherPermissionData)
    {
        return this.GetOrderedIdents(permissionData).SequenceEqual(this.GetOrderedIdents(otherPermissionData), this.RestrictionGroupComparer);
    }

    protected IEnumerable<IGrouping<TSecurityContextType, TSecurityContextObjectIdent>> GetOrderedIdents(
        PermissionData<TPermission, TPermissionRestriction> permissionData)
    {
        return

            from permissionRestriction in permissionData.Restrictions

            orderby restrictionBindingInfo.SecurityContextObjectId

            group restrictionBindingInfo.SecurityContextObjectId.Getter(permissionRestriction) by restrictionBindingInfo.SecurityContextType.Getter(
                permissionRestriction)

            into g

            orderby securityContextTypeIdentityInfo.Id.Getter(g.Key)

            select g;
    }

    public int GetHashCode(PermissionData<TPermission, TPermissionRestriction> permissionData)
    {
        return permissionData.Restrictions.Count ^ generalBindingInfo.SecurityRole.Getter(permissionData.Permission).GetHashCode();
    }
}