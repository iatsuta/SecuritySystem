namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionRestrictionBindingInfoSource : IGeneralPermissionRestrictionBindingInfoSource
{
    private readonly IReadOnlyDictionary<Type, GeneralPermissionRestrictionBindingInfo> permissionDict;

    private readonly IReadOnlyDictionary<Type, GeneralPermissionRestrictionBindingInfo> permissionRestrictionDict;

    public GeneralPermissionRestrictionBindingInfoSource(IEnumerable<GeneralPermissionRestrictionBindingInfo> bindingInfoList)
    {
        var cache = bindingInfoList.ToList();

        this.permissionDict = cache.ToDictionary(v => v.PermissionType);
        this.permissionRestrictionDict = cache.ToDictionary(v => v.PermissionRestrictionType);
    }


    public GeneralPermissionRestrictionBindingInfo GetForPermission(Type permissionType) => this.permissionDict[permissionType];

    public GeneralPermissionRestrictionBindingInfo GetForPermissionRestriction(Type permissionRestrictionType) => this.permissionRestrictionDict[permissionRestrictionType];
}