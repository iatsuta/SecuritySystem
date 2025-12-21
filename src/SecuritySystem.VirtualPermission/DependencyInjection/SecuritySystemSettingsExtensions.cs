using System.Linq.Expressions;

using CommonFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
            where TPrincipal : class
            where TPermission : class =>
            securitySystemSettings

                .AddPermissionSystem(sp => ActivatorUtilities.CreateInstance<VirtualPermissionSystemFactory<TPrincipal, TPermission>>(sp, bindingInfo))

                .AddExtensions(sc => sc.AddScoped<IPrincipalSourceService>(sp =>
                        ActivatorUtilities.CreateInstance<VirtualPrincipalSourceService<TPrincipal, TPermission>>(
                            sp,
                            bindingInfo))

                    .TryAddSingleton<IVirtualPermissionBindingInfoValidator, VirtualPermissionBindingInfoValidator>());

        public ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(SecurityRole securityRole,
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Func<VirtualPermissionBindingInfo<TPrincipal, TPermission>, VirtualPermissionBindingInfo<TPrincipal, TPermission>>? initFunc = null)
            where TPrincipal : class
            where TPermission : class
        {
            var bindingInfo =
                (initFunc ?? (v => v)).Invoke(
                    new VirtualPermissionBindingInfo<TPrincipal, TPermission>(securityRole)
                        { IsReadonly = true, Principal = principalPath.ToPropertyAccessors() });

            return securitySystemSettings.AddVirtualPermission(bindingInfo);
        }
    }
}