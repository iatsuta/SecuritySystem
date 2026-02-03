using CommonFramework;
using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.Management;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddVirtualPermission(
            PermissionBindingInfo bindingInfo,
            IReadOnlyList<VirtualPermissionBindingInfo> virtualBindingInfoList)
        {
            foreach (var virtualBindingInfo in virtualBindingInfoList)
            {
                securitySystemSettings.AddPermissionSystem(serviceProxyFactory =>
                    serviceProxyFactory.Create<IPermissionSystemFactory, VirtualPermissionSystemFactory>(virtualBindingInfo));
            }

            return securitySystemSettings.AddExtensions(services =>
            {
                services.AddSingleton(bindingInfo);
                services.TryAddSingleton<IVirtualPermissionBindingInfoValidator, VirtualPermissionBindingInfoValidator>();

                foreach (var virtualBindingInfo in virtualBindingInfoList)
                {
                    var serviceType = typeof(VirtualPrincipalSourceService<>).MakeGenericType(bindingInfo.PermissionType);

                    services.AddScopedFrom<IPrincipalSourceService, IServiceProxyFactory>(factory =>
                        factory.Create<IPrincipalSourceService>(serviceType, virtualBindingInfo));
                }
            });
        }

        public ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(
            PropertyAccessors<TPermission, TPrincipal> principalAccessors,
            Action<IVirtualBindingInfoRootSettingsBuilder<TPermission>> initAction)
            where TPrincipal : class
            where TPermission : class
        {
            var bindingInfo = new PermissionBindingInfo<TPermission, TPrincipal> { IsReadonly = true, Principal = principalAccessors };

            var builder = new VirtualBindingInfoRootSettingsBuilder<TPermission>();

            initAction.Invoke(builder);

            return securitySystemSettings.AddVirtualPermission(bindingInfo, builder.VirtualPermissionBindingInfoList.ToArray());
        }

        public ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Action<IVirtualBindingInfoRootSettingsBuilder<TPermission>> initAction)
            where TPrincipal : class
            where TPermission : class =>
            securitySystemSettings.AddVirtualPermission(principalPath.ToPropertyAccessors(), initAction);
    }
}