using CommonFramework;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission.AvailableSecurity;
using SecuritySystem.GeneralPermission.Initialize;
using SecuritySystem.GeneralPermission.Validation;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
        public ISecuritySystemSettings AddGeneralPermission(GeneralPermissionBindingInfo bindingInfo,
            GeneralPermissionRestrictionBindingInfo restrictionBindingInfo)
        {
            return securitySystemSettings
                .AddPermissionSystem(sp => new GeneralPermissionSystemFactory(sp, bindingInfo))
                .AddExtensions(services => services

                    .AddSingleton(bindingInfo)
                    .AddSingleton(restrictionBindingInfo)

                    .AddSingleton(typeof(IPermissionRestrictionTypeFilterFactory<>), typeof(PermissionRestrictionTypeFilterFactory<>))
                    .AddScoped(typeof(IPermissionRestrictionFilterFactory<>), typeof(PermissionRestrictionFilterFactory<>))
                    .AddSingleton(typeof(IRawPermissionConverter<>), typeof(RawPermissionConverter<>))
                    .AddScoped(typeof(IPrincipalDomainService<>), typeof(PrincipalDomainService<>))
                    .AddSingleton(typeof(IPermissionSecurityRoleFilterFactory<>), typeof(PermissionSecurityRoleFilterFactory<>))
                    .AddScoped(typeof(IAvailablePermissionFilterFactory<>), typeof(AvailablePermissionFilterFactory<>))
                    .AddScoped(typeof(IPermissionFilterFactory<>), typeof(PermissionFilterFactory<>))
                    .AddScoped(typeof(IAvailablePermissionSource<>), typeof(AvailablePermissionSource<>))

                    .AddScoped(typeof(ISecurityRoleInitializer<>), typeof(SecurityRoleInitializer<>))
                    .AddScoped(typeof(ISecurityContextInitializer<>), typeof(SecurityContextInitializer<>))

                    .AddScoped(typeof(ITypedPrincipalConverter<>), typeof(TypedPrincipalConverter<>))

                    .AddScoped(typeof(IAvailablePrincipalSource<>), typeof(AvailablePrincipalSource<>))

                    .AddScoped<ISecurityValidator<PrincipalData>, PrincipalRootValidator>()

                    .AddSingleton<InitializerSettings>()

                    .AddSingleton<IGeneralPermissionBindingInfoSource, GeneralPermissionBindingInfoSource>()
                    .AddSingleton<IGeneralPermissionRestrictionBindingInfoSource, GeneralPermissionRestrictionBindingInfoSource>()

                    .AddScoped(typeof(IPrincipalSourceService), typeof(GeneralPrincipalSourceService<>).MakeGenericType(bindingInfo.PrincipalType))
                )
                .Pipe(!bindingInfo.IsReadonly, v => v.SetPrincipalManagementService<GeneralPrincipalManagementService>());
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
            var settings = new GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole>();

            setupAction?.Invoke(settings);

            var bindingInfo = new GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole>
            {
                Principal = principalPath.ToPropertyAccessors(),
                SecurityRole = securityRolePath.ToPropertyAccessors(),
            }.Pipe(settings.ApplyOptionalPaths);

            var restrictionBindingInfo =
                new GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission>
                {
                    Permission = permissionPath.ToPropertyAccessors(),
                    SecurityContextType = securityContextTypePath.ToPropertyAccessors(),
                    SecurityContextObjectId = securityContextObjectIdPath.ToPropertyAccessors()
                };

            return securitySystemSettings.AddGeneralPermission(bindingInfo, restrictionBindingInfo);
        }
    }
}