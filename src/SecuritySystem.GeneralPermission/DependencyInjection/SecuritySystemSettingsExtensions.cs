using CommonFramework;
using SecuritySystem.DependencyInjection;
using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            PropertyAccessors<TPermission, TPrincipal> principalAccessors,
            PropertyAccessors<TPermission, TSecurityRole> securityRoleAccessors,
            PropertyAccessors<TPermissionRestriction, TPermission> permissionAccessors,
            PropertyAccessors<TPermissionRestriction, TSecurityContextType> securityContextTypeAccessors,
            PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> securityContextObjectIdAccessors,
            Action<IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull
        {
            var settings = new GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>();

            setupAction?.Invoke(settings);

            settings.Initialize(securitySystemSettings, principalAccessors, securityRoleAccessors, permissionAccessors, securityContextTypeAccessors,
                securityContextObjectIdAccessors);

            return securitySystemSettings;
        }

        public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Expression<Func<TPermission, TSecurityRole>> securityRolePath,
            Expression<Func<TPermissionRestriction, TPermission>> permissionPath,
            Expression<Func<TPermissionRestriction, TSecurityContextType>> securityContextTypePath,
            Expression<Func<TPermissionRestriction, TSecurityContextObjectIdent>> securityContextObjectIdPath,
            Action<IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull =>
            securitySystemSettings.AddGeneralPermission(
                principalPath.ToPropertyAccessors(),
                securityRolePath.ToPropertyAccessors(),
                permissionPath.ToPropertyAccessors(),
                securityContextTypePath.ToPropertyAccessors(),
                securityContextObjectIdPath.ToPropertyAccessors(),
                setupAction);
    }
}