using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystemFactory : IPermissionSystemFactory
{
	private readonly IServiceProvider serviceProvider;

	private readonly VirtualPermissionBindingInfo virtualBindingInfo;

	public VirtualPermissionSystemFactory(
		IServiceProvider serviceProvider,
		VirtualPermissionBindingInfo virtualBindingInfo,
		IVirtualPermissionBindingInfoValidator validator)
	{
		this.serviceProvider = serviceProvider;
		this.virtualBindingInfo = virtualBindingInfo;

		validator.Validate(this.virtualBindingInfo);
	}

	public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential)
    {
        var permissionSystemType = typeof(VirtualPermissionSystem<>).MakeGenericType(virtualBindingInfo.PermissionType);

        return (IPermissionSystem)ActivatorUtilities.CreateInstance(serviceProvider, permissionSystemType, this.virtualBindingInfo, securityRuleCredential);
    }
}