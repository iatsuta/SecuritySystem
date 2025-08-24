using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.VirtualPermission;

public static class SecuritySystemSettingsExtensions
{
    public static ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(
        this ISecuritySystemSettings securitySystemSettings,
        VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
        where TPrincipal : class
        where TPermission : class =>
        securitySystemSettings

            .AddPermissionSystem(
                sp => ActivatorUtilities.CreateInstance<VirtualPermissionSystemFactory<TPrincipal, TPermission>>(sp, bindingInfo))

            .AddExtensions(
                sc => sc.AddScoped<IPrincipalSourceService>(
                            sp => ActivatorUtilities.CreateInstance<VirtualPrincipalSourceService<TPrincipal, TPermission>>(
                                sp,
                                bindingInfo))

                        .TryAddSingleton<IVirtualPermissionBindingInfoValidator, VirtualPermissionBindingInfoValidator>());

    public static ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(
        this ISecuritySystemSettings securitySystemSettings,
        SecurityRole securityRole,
        Expression<Func<TPermission, TPrincipal>> principalPath,
        Expression<Func<TPrincipal, string>> principalNamePath,
        Func<VirtualPermissionBindingInfo<TPrincipal, TPermission>, VirtualPermissionBindingInfo<TPrincipal, TPermission>>? initFunc = null)
        where TPrincipal : class
        where TPermission : class
    {
        var bindingInfo =
            (initFunc ?? (v => v)).Invoke(
                new VirtualPermissionBindingInfo<TPrincipal, TPermission>(securityRole, principalPath, principalNamePath));

        return securitySystemSettings.AddVirtualPermission(bindingInfo);
    }
}
