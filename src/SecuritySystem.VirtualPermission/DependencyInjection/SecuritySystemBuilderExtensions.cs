using CommonFramework;

using SecuritySystem.DependencyInjection;

using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission.DependencyInjection;

public static class SecuritySystemBuilderExtensions
{
    extension(ISecuritySystemBuilder securitySystemBuilder)
    {
        public ISecuritySystemBuilder AddVirtualPermission<TPrincipal, TPermission>(
            PropertyAccessors<TPermission, TPrincipal> principalAccessors,
            Action<IVirtualPermissionRootBuilder<TPermission>> setupAction)
            where TPrincipal : class
            where TPermission : class
        {
            var builder = new VirtualPermissionRootBuilder<TPrincipal, TPermission>();

            setupAction.Invoke(builder);

            builder.Initialize(securitySystemBuilder, principalAccessors);

            return securitySystemBuilder;
        }

        public ISecuritySystemBuilder AddVirtualPermission<TPrincipal, TPermission>(
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Action<IVirtualPermissionRootBuilder<TPermission>> setupAction)
            where TPrincipal : class
            where TPermission : class =>
            securitySystemBuilder.AddVirtualPermission(
                principalPath.ToPropertyAccessors(), setupAction);
    }
}