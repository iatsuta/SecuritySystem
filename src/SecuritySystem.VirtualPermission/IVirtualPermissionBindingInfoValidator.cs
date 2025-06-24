namespace SecuritySystem.VirtualPermission;

public interface IVirtualPermissionBindingInfoValidator
{
    void Validate<TPrincipal, TPermission>(VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo);
}
