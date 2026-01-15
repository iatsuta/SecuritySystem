using CommonFramework.DependencyInjection;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystemFactory : IPermissionSystemFactory
{
    private readonly IServiceProxyFactory serviceProxyFactory;

    private readonly VirtualPermissionBindingInfo virtualBindingInfo;

    public VirtualPermissionSystemFactory(
        IServiceProxyFactory serviceProxyFactory,
        VirtualPermissionBindingInfo virtualBindingInfo,
        IVirtualPermissionBindingInfoValidator validator)
    {
        this.serviceProxyFactory = serviceProxyFactory;
        this.virtualBindingInfo = virtualBindingInfo;

        validator.Validate(this.virtualBindingInfo);
    }

    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential)
    {
        var permissionSystemType = typeof(VirtualPermissionSystem<>).MakeGenericType(virtualBindingInfo.PermissionType);

        return serviceProxyFactory.Create<IPermissionSystem>(permissionSystemType, this.virtualBindingInfo, securityRuleCredential);
    }
}