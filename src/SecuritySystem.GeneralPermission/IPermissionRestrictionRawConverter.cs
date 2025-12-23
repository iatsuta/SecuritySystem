namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionRawConverter<in TPermissionRestriction>
{
    Dictionary<Type, Array> Convert(IEnumerable<TPermissionRestriction> permissionRestrictions);
}