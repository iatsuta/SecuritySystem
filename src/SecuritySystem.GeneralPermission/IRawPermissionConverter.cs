namespace SecuritySystem.GeneralPermission;

public interface IRawPermissionConverter<in TPermissionRestriction>
{
    Dictionary<Type, Array> ConvertPermission(DomainSecurityRule.RoleBaseSecurityRule securityRule, IReadOnlyList<TPermissionRestriction> restrictions, IEnumerable<Type> securityContextTypes);
}