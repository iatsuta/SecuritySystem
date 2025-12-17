using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission.AvailableSecurity;
using SecuritySystem.GeneralPermission.Initialize;
using SecuritySystem.GeneralPermission.Validation;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
                bindingInfo,
            Action<IGeneralPermissionSettings<TPermission, TSecurityRole>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull
        {
            var settings = new GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole>();

            setupAction?.Invoke(settings);

            var finalBindingInfo = settings.ApplyOptionalPaths(bindingInfo);

            return securitySystemSettings
                .AddPermissionSystem(sp => ActivatorUtilities
                    .CreateInstance<
                        GeneralPermissionSystemFactory<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                            TSecurityContextObjectIdent>>(sp, finalBindingInfo))
                .AddExtensions(sc => sc
                    .AddSingleton(finalBindingInfo)
                    .AddSingleton<GeneralPermissionBindingInfo>(finalBindingInfo)
                    .AddSingletonFrom<GeneralPermissionBindingInfo<TPrincipal, TPermission>, GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole
                        , TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
                    .AddSingletonFrom<GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole>, GeneralPermissionBindingInfo<TPrincipal, TPermission
                        , TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
                    .AddSingletonFrom<IPermissionRestrictionToPermissionInfo<TPermissionRestriction, TPermission>, GeneralPermissionBindingInfo<TPrincipal,
                        TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
                    .AddSingletonFrom<IPermissionToPrincipalInfo<TPermission, TPrincipal>, GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole,
                        TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
                    .AddSingletonFrom<IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
                        , GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                            TSecurityContextObjectIdent>>()
                    .AddSingletonFrom<IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType>,
                        GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                            TSecurityContextObjectIdent>>()

                    .AddSingleton(typeof(IPermissionRestrictionTypeFilterFactory<>), typeof(PermissionRestrictionTypeFilterFactory<>))
                    .AddScoped(typeof(IPermissionRestrictionFilterFactory<>), typeof(PermissionRestrictionFilterFactory<>))
                    .AddSingleton(typeof(IRawPermissionConverter<>), typeof(RawPermissionConverter<>))
                    .AddScoped(typeof(IPrincipalDomainService<>), typeof(PrincipalDomainService<>))
                    .AddSingleton(typeof(IPermissionSecurityRoleFilterFactory<>), typeof(PermissionSecurityRoleFilterFactory<>))
                    .AddScoped(typeof(IAvailablePermissionFilterFactory<>), typeof(AvailablePermissionFilterFactory<>))
                    .AddScoped(typeof(IPermissionFilterFactory<>), typeof(PermissionFilterFactory<>))
                    .AddScoped(typeof(IAvailablePermissionSource<>), typeof(AvailablePermissionSource<>))
                    .AddScoped<IAvailableSecurityRoleSource, GeneralAvailableSecurityRoleSource>()

                    .AddScoped(typeof(ISecurityRoleInitializer<>), typeof(SecurityRoleInitializer<>))
                    .AddScoped(typeof(ISecurityContextInitializer<>), typeof(SecurityContextInitializer<>))

                    .AddScoped<ISecurityValidator<PrincipalData<TPrincipal, TPermission, TPermissionRestriction>>, PrincipalRootValidator<TPrincipal, TPermission, TPermissionRestriction>>()

                    .AddSingleton<InitializerSettings>()
                );
        }

        public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
            TSecurityContextObjectIdent>(
            Expression<Func<TPermission, TPrincipal>> principalPath,
            Expression<Func<TPermission, TSecurityRole>> securityRolePath,
            Expression<Func<TPermissionRestriction, TPermission>> permissionPath,
            Expression<Func<TPermissionRestriction, TSecurityContextType>> securityContextTypePath,
            Expression<Func<TPermissionRestriction, TSecurityContextObjectIdent>> securityContextObjectIdPath,
            Action<IGeneralPermissionSettings<TPermission, TSecurityRole>>? setupAction = null)
            where TPrincipal : class
            where TPermission : class
            where TSecurityRole : class
            where TPermissionRestriction : class
            where TSecurityContextType : class
            where TSecurityContextObjectIdent : notnull
        {
            return securitySystemSettings.AddGeneralPermission(
                new GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                    TSecurityContextObjectIdent>
                {
                    Principal = principalPath.ToPropertyAccessors(),
                    SecurityRole = securityRolePath.ToPropertyAccessors(),
                    Permission = permissionPath.ToPropertyAccessors(),
                    SecurityContextType = securityContextTypePath.ToPropertyAccessors(),
                    SecurityContextObjectId = securityContextObjectIdPath.ToPropertyAccessors()
                },
                setupAction);
        }
    }
}