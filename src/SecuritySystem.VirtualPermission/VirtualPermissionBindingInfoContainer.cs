namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionBindingInfoContainer<TPrincipal, TPermission>(
    IVirtualPermissionBindingInfoValidator validator,
    VirtualPermissionBindingInfo<TPrincipal, TPermission> rawBindingInfo)
{
    private readonly Lazy<VirtualPermissionBindingInfo<TPrincipal, TPermission>> lazyValidatedBindingInfo = new(() =>
    {
        validator.Validate(rawBindingInfo);

        return rawBindingInfo;
    });

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> ValidatedBindingInfo => lazyValidatedBindingInfo.Value;
}