using CommonFramework;
using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission.Initialize;
using SecuritySystem.GeneralPermission.Validation;
using SecuritySystem.GeneralPermission.Validation.Permission;
using SecuritySystem.GeneralPermission.Validation.PermissionRestriction;
using SecuritySystem.GeneralPermission.Validation.Principal;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddGeneralPermission(
            PermissionBindingInfo bindingInfo,
            GeneralPermissionBindingInfo generalBindingInfo,
            GeneralPermissionRestrictionBindingInfo restrictionBindingInfo,
            Type? permissionEqualityComparerType,
            Type? permissionManagementServiceType)
        {
            return securitySystemSettings
                .AddGeneralServices()

                .AddPermissionSystem(sp => new GeneralPermissionSystemFactory(sp, bindingInfo))
                .AddExtensions(services => services

                    .AddSingleton(bindingInfo)
                    .AddSingleton(generalBindingInfo)
                    .AddSingleton(restrictionBindingInfo)

                    .PipeMaybe(permissionEqualityComparerType, (sc, v) => sc
                        .AddScoped(
                            typeof(IPermissionEqualityComparer<,>)
                                .MakeGenericType(restrictionBindingInfo.PermissionType, restrictionBindingInfo.PermissionRestrictionType), v))

                    .PipeMaybe(permissionManagementServiceType, (sc, v) => sc
                        .AddScoped(
                            typeof(IPermissionManagementService<,,>)
                                .MakeGenericType(bindingInfo.PrincipalType, restrictionBindingInfo.PermissionType, restrictionBindingInfo.PermissionRestrictionType), v))

                    .AddScoped(typeof(IPrincipalSourceService), typeof(GeneralPrincipalSourceService<>).MakeGenericType(bindingInfo.PrincipalType)));
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
            where TSecurityContextObjectIdent : notnull
        {
            var settings = new GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>();

            setupAction?.Invoke(settings);

            var bindingInfo = new PermissionBindingInfo<TPermission, TPrincipal>
            {
                Principal = principalPath.ToPropertyAccessors(),
            }.Pipe(settings.ApplyOptionalPaths);

            var generalBindingInfo = new GeneralPermissionBindingInfo<TPermission, TSecurityRole>
            {
                SecurityRole = securityRolePath.ToPropertyAccessors(),
            }.Pipe(settings.ApplyGeneralOptionalPaths);

            var restrictionBindingInfo =
                new GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission>
                {
                    Permission = permissionPath.ToPropertyAccessors(),
                    SecurityContextType = securityContextTypePath.ToPropertyAccessors(),
                    SecurityContextObjectId = securityContextObjectIdPath.ToPropertyAccessors()
                };

            return securitySystemSettings.AddGeneralPermission(
                bindingInfo,
                generalBindingInfo,
                restrictionBindingInfo,
                settings.PermissionEqualityComparerType,
                settings.PermissionManagementServiceType);
        }

        private ISecuritySystemSettings AddGeneralServices()
        {
            return securitySystemSettings
                .AddExtensions(services =>
                {
                    if (services.AlreadyInitialized<IGeneralPermissionBindingInfoSource, GeneralPermissionBindingInfoSource>())
                    {
                        return;
                    }

                    services
                        .AddSingleton(typeof(IPermissionRestrictionTypeFilterFactory<>), typeof(PermissionRestrictionTypeFilterFactory<>))
                        .AddScoped(typeof(IPermissionRestrictionFilterFactory<>), typeof(PermissionRestrictionFilterFactory<>))
                        .AddScoped(typeof(IRawPermissionConverter<>), typeof(RawPermissionConverter<>))
                        .AddSingleton(typeof(IPermissionSecurityRoleFilterFactory<>), typeof(PermissionSecurityRoleFilterFactory<>))
                        .AddSingleton(typeof(IPermissionSecurityRoleIdentsFilterFactory<>), typeof(PermissionSecurityRoleIdentsFilterFactory<>))
                        .AddScoped(typeof(IPermissionFilterFactory<>), typeof(PermissionFilterFactory<>))
                        .AddScoped<ISecurityRoleInitializer, SecurityRoleInitializer>()
                        .AddScoped(typeof(ISecurityRoleInitializer<>), typeof(SecurityRoleInitializer<>))
                        .AddScoped<ISecurityContextInitializer, SecurityContextInitializer>()
                        .AddScoped(typeof(ISecurityContextInitializer<>), typeof(SecurityContextInitializer<>))
                        .AddScoped(typeof(IManagedPrincipalConverter<>), typeof(ManagedPrincipalConverter<>))
                        .AddScoped(typeof(IDisplayPermissionService<,>), typeof(DisplayPermissionService<,>))

                        .AddSingleton(typeof(IPermissionEqualityComparer<,>), typeof(PermissionEqualityComparer<,>))

                        .AddKeyedScoped(typeof(IPrincipalValidator<,,>), "Root", typeof(PrincipalRootValidator<,,>))
                        .AddScoped(typeof(IPrincipalValidator<,,>), typeof(PrincipalUniquePermissionValidator<,,>))
                        .AddKeyedScoped(typeof(IPermissionValidator<,>), "Root", typeof(PermissionRootValidator<,>))
                        .AddSingleton(typeof(IPermissionValidator<,>), typeof(PermissionRequiredContextValidator<,>))
                        .AddScoped(typeof(IPermissionValidator<,>), typeof(PermissionDelegationValidator<,>))
                        .AddKeyedScoped(typeof(IPermissionRestrictionValidator<>), "Root", typeof(PermissionRestrictionRootValidator<>))
                        .AddSingleton(typeof(IPermissionRestrictionValidator<>), typeof(AllowedTypePermissionRestrictionValidator<>))
                        .AddScoped(typeof(IPermissionRestrictionValidator<>), typeof(ExistsPermissionRestrictionValidator<>))
                        .AddScoped(typeof(IPermissionRestrictionValidator<>), typeof(AllowedFilterPermissionRestrictionValidator<>))
                        .AddScoped(typeof(IPermissionLoader<,>), typeof(PermissionLoader<,>))
                        .AddScoped(typeof(IPermissionRestrictionLoader<,>), typeof(PermissionRestrictionLoader<,>))
                        .AddScoped(typeof(IRawPermissionRestrictionLoader<>), typeof(RawPermissionRestrictionLoader<>))
                        .AddSingleton(typeof(IPermissionRestrictionRawConverter<>), typeof(PermissionRestrictionRawConverter<>))
                        .AddSingleton(typeof(IPermissionSecurityRoleResolver<>), typeof(PermissionSecurityRoleResolver<>))
                        .AddSingleton(typeof(IPermissionRestrictionSecurityContextTypeResolver<>), typeof(PermissionRestrictionSecurityContextTypeResolver<>))
                        .AddSingleton<InitializerSettings>()
                        .AddSingleton<IGeneralPermissionBindingInfoSource, GeneralPermissionBindingInfoSource>()
                        .AddSingleton<IGeneralPermissionRestrictionBindingInfoSource, GeneralPermissionRestrictionBindingInfoSource>()
                        .AddScoped(typeof(IPermissionManagementService<,,>), typeof(PermissionManagementService<,,>));
                })

                .SetPrincipalManagementService<GeneralPrincipalManagementService>();
        }
    }
}