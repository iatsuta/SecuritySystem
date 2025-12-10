using SecuritySystem.DependencyInjection;

using System.Linq.Expressions;
using CommonFramework;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> info,
            Action<IGeneralPermissionSettings<TPermission>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull
        {
            return securitySystemSettings
                .AddPermissionSystem<GeneralPermissionSystemFactory>();
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
                new GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
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