using CommonFramework;

using SecuritySystem.DependencyInjection;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(
            PropertyAccessors<TPermission, TPrincipal> principalAccessors,
            Action<IVirtualBindingInfoRootSettingsBuilder<TPermission>> setupAction)
            where TPrincipal : class
            where TPermission : class
        {
            var builder = new VirtualBindingInfoRootSettingsBuilder<TPrincipal, TPermission>();

            setupAction.Invoke(builder);

            builder.Initialize(securitySystemSettings, principalAccessors);

            return securitySystemSettings;
        }

        public ISecuritySystemSettings AddVirtualPermission<TPrincipal, TPermission>(
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Action<IVirtualBindingInfoRootSettingsBuilder<TPermission>> setupAction)
            where TPrincipal : class
            where TPermission : class =>
            securitySystemSettings.AddVirtualPermission(
                principalPath.ToPropertyAccessors(), setupAction);
    }
}