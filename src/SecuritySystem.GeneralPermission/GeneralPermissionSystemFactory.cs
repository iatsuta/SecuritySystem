using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSystemFactory(IServiceProvider serviceProvider, GeneralPermissionBindingInfo bindingInfo) : IPermissionSystemFactory
{
    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential)
    {
        var permissionSystemType = typeof(GeneralPermissionSystem<>).MakeGenericType(bindingInfo.PermissionType);

        return (IPermissionSystem)ActivatorUtilities.CreateInstance(serviceProvider, permissionSystemType, securityRuleCredential);
    }
}