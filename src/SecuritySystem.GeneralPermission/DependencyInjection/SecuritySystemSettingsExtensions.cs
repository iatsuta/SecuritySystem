using SecuritySystem.DependencyInjection;

using System.Linq.Expressions;

using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
                bindingInfo,
            Action<IGeneralPermissionSettings<TPermission>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull
        {
            var settings = new GeneralPermissionSettings<TPrincipal, TPermission>();

            setupAction?.Invoke(settings);

            var finalBindingInfo = settings.ApplyOptionalPaths(bindingInfo);

            return securitySystemSettings
                .AddPermissionSystem(sp => ActivatorUtilities
                    .CreateInstance<
                        GeneralPermissionSystemFactory<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                            TSecurityContextObjectIdent>>(sp, finalBindingInfo));
        }

        public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Expression<Func<TPermission, TSecurityRole>> securityRolePath,
            Expression<Func<TPermissionRestriction, TPermission>> permissionPath,
            Expression<Func<TPermissionRestriction, TSecurityContextType>> securityContextTypePath,
            Expression<Func<TPermissionRestriction, TSecurityContextObjectIdent>> securityContextObjectIdPath,
            Action<IGeneralPermissionSettings<TPermission>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull
        {
            return securitySystemSettings.AddGeneralPermission(
                new GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                    TSecurityContextObjectIdent>(
                    principalPath.ToPropertyAccessors(),
                    securityRolePath.ToPropertyAccessors(),
                    permissionPath.ToPropertyAccessors(),
                    securityContextTypePath.ToPropertyAccessors(),
                    securityContextObjectIdPath.ToPropertyAccessors()),
                setupAction);
        }
    }
}