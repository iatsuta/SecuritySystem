namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionBindingInfoSource : IGeneralPermissionBindingInfoSource
{
    private readonly IReadOnlyDictionary<Type, GeneralPermissionBindingInfo> permissionDict;

    private readonly IReadOnlyDictionary<Type, GeneralPermissionBindingInfo> securityRoleDict;

    public GeneralPermissionBindingInfoSource(IEnumerable<GeneralPermissionBindingInfo> bindingInfoList)
    {
        var cache = bindingInfoList.ToList();

        this.permissionDict = cache.ToDictionary(v => v.PermissionType);
        this.securityRoleDict = cache.ToDictionary(v => v.SecurityRoleType);
    }

    public GeneralPermissionBindingInfo GetForPermission(Type permissionType) => this.permissionDict[permissionType];

    public GeneralPermissionBindingInfo GetForSecurityRole(Type securityRoleType) => this.securityRoleDict[securityRoleType];
}