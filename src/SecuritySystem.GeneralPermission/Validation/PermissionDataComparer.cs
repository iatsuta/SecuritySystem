using CommonFramework;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission.Validation;

//public class PermissionDataComparer<TPermission, TPermissionRestriction> : IEqualityComparer<PermissionData<TPermission, TPermissionRestriction>>
//{

//}

public class PermissionDataComparer<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> restrictionBindingInfo)
    : IEqualityComparer<PermissionData<TPermission, TPermissionRestriction>>
    where TPrincipal : class
    where TPermission : class
    where TSecurityRole : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
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
               && (bindingInfo.PermissionPeriod == null
                   || bindingInfo.PermissionPeriod.Getter(permissionData.Permission)
                       .IsIntersected(bindingInfo.PermissionPeriod.Getter(otherPermissionData.Permission)))
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

            orderby g.Key

            select g;
    }

    public int GetHashCode(PermissionData<TPermission, TPermissionRestriction> permissionData)
    {
        return permissionData.Restrictions.Count ^ generalBindingInfo.SecurityRole.Getter(permissionData.Permission).GetHashCode();
    }
}