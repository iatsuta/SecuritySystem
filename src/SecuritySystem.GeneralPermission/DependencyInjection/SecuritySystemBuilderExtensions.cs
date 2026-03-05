using CommonFramework;
using CommonFramework.DependencyInjection;

using SecuritySystem.DependencyInjection;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemBuilderExtensions
{
    extension(ISecuritySystemBuilder securitySystemBuilder)
    {
        public ISecuritySystemBuilder AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            PropertyAccessors<TPermission, TPrincipal> principalAccessors,
            PropertyAccessors<TPermission, TSecurityRole> securityRoleAccessors,
            PropertyAccessors<TPermissionRestriction, TPermission> permissionAccessors,
            PropertyAccessors<TPermissionRestriction, TSecurityContextType> securityContextTypeAccessors,
            PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> securityContextObjectIdAccessors,
            Action<IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull =>
            securitySystemBuilder
                .Initialize<ISecuritySystemBuilder,
                    GeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                        TSecurityContextObjectIdent>>
                (new (principalAccessors, securityRoleAccessors, permissionAccessors, securityContextTypeAccessors, securityContextObjectIdAccessors),
                    setupAction);

        public ISecuritySystemBuilder AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Expression<Func<TPermission, TSecurityRole>> securityRolePath,
            Expression<Func<TPermissionRestriction, TPermission>> permissionPath,
            Expression<Func<TPermissionRestriction, TSecurityContextType>> securityContextTypePath,
            Expression<Func<TPermissionRestriction, TSecurityContextObjectIdent>> securityContextObjectIdPath,
            Action<IGeneralPermissionBuilder<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull =>
            securitySystemBuilder.AddGeneralPermission(
                principalPath.ToPropertyAccessors(),
                securityRolePath.ToPropertyAccessors(),
                permissionPath.ToPropertyAccessors(),
                securityContextTypePath.ToPropertyAccessors(),
                securityContextObjectIdPath.ToPropertyAccessors(),
                setupAction);
    }
}