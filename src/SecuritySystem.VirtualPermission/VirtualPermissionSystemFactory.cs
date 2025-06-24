using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystemFactory<TPrincipal, TPermission> : IPermissionSystemFactory
    where TPrincipal : IIdentityObject<Guid>
    where TPermission : class, IIdentityObject<Guid>
{
    private readonly IServiceProvider serviceProvider;

    private readonly VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo;

    public VirtualPermissionSystemFactory(
        IServiceProvider serviceProvider,
        VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo,
        IVirtualPermissionBindingInfoValidator validator)
    {
        this.serviceProvider = serviceProvider;
        this.bindingInfo = bindingInfo;

        validator.Validate(this.bindingInfo);
    }

    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential) =>
        ActivatorUtilities.CreateInstance<VirtualPermissionSystem<TPrincipal, TPermission>>(
            this.serviceProvider,
            this.bindingInfo,
            securityRuleCredential);
}
