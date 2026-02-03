namespace SecuritySystem.GeneralPermission;

public interface IRawPermissionConverter<in TPermissionRestriction>
{
    Dictionary<Type, Array> ConvertPermission(DomainSecurityRule.RoleBaseSecurityRule securityRule, IEnumerable<TPermissionRestriction> restrictions, IEnumerable<Type> securityContextTypes);
}