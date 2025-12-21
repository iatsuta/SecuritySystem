namespace SecuritySystem.Services;

public class PermissionBindingInfoSource : IPermissionBindingInfoSource
{
    private readonly IReadOnlyDictionary<Type, PermissionBindingInfo> permissionDict;

    private readonly IReadOnlyDictionary<Type, PermissionBindingInfo> principalDict;

    public PermissionBindingInfoSource(IEnumerable<PermissionBindingInfo> bindingInfoList)
    {
        var cache = bindingInfoList.ToList();

        this.principalDict = cache.ToDictionary(v => v.PrincipalType);
        this.permissionDict = cache.ToDictionary(v => v.PermissionType);
    }

    public PermissionBindingInfo GetForPermission(Type permissionType) => this.permissionDict[permissionType];

    public PermissionBindingInfo GetForPrincipal(Type principalType) => this.principalDict[principalType];
}