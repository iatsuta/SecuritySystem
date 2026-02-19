using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission.Validation;
using SecuritySystem.GeneralPermission.Initialize;
using SecuritySystem.GeneralPermission.Validation.Permission;
using SecuritySystem.GeneralPermission.Validation.PermissionRestriction;
using SecuritySystem.GeneralPermission.Validation.Principal;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public class GeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> : IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction>
    where TPermission : class
    where TSecurityRole : notnull
    where TPrincipal : class
{
    private PropertyAccessors<TPermission, DateTime?>? startDateAccessors;

    private PropertyAccessors<TPermission, DateTime?>? endDatedAccessors;

    private Expression<Func<TPermission, string>>? commentPath;

    private Expression<Func<TSecurityRole, string>>? descriptionPath;

    private Expression<Func<TPermission, TPermission?>>? delegatedFromPath;

    private bool? isReadonly;

    private Type? permissionEqualityComparerType;

    private Type? permissionManagementServiceType;


    public void Initialize(ISecuritySystemSettings securitySystemSettings,
        PropertyAccessors<TPermission, TPrincipal> principalAccessors,
        PropertyAccessors<TPermission, TSecurityRole> securityRoleAccessors,
        PropertyAccessors<TPermissionRestriction, TPermission> permissionAccessors,
        PropertyAccessors<TPermissionRestriction, TSecurityContextType> securityContextTypeAccessors,
        PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> securityContextObjectIdAccessors)
    {
        this.RegisterGeneralServices(securitySystemSettings);

        var bindingInfo = new PermissionBindingInfo<TPermission, TPrincipal>
        {
            Principal = principalAccessors,
        }.Pipe(this.ApplyOptionalPaths);

        var generalBindingInfo = new GeneralPermissionBindingInfo<TPermission, TSecurityRole>
        {
            SecurityRole = securityRoleAccessors,
        }.Pipe(this.ApplyGeneralOptionalPaths);

        var restrictionBindingInfo =
            new GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission>
            {
                Permission = permissionAccessors,
                SecurityContextType = securityContextTypeAccessors,
                SecurityContextObjectId = securityContextObjectIdAccessors
            };

        securitySystemSettings
            .AddPermissionSystem(sp => new GeneralPermissionSystemFactory(sp, bindingInfo))
            .AddExtensions(services =>
            {
                services
                    .AddSingleton<PermissionBindingInfo>(bindingInfo)
                    .AddSingleton<GeneralPermissionBindingInfo>(generalBindingInfo)
                    .AddSingleton<GeneralPermissionRestrictionBindingInfo>(restrictionBindingInfo)

                    .AddScoped(typeof(IPrincipalSourceService), typeof(GeneralPrincipalSourceService<TPrincipal>));

                if (this.permissionEqualityComparerType != null)
                {
                    services.AddScoped(typeof(IPermissionEqualityComparer<TPermission, TPermissionRestriction>), this.permissionEqualityComparerType);
                }

                if (this.permissionManagementServiceType != null)
                {
                    services.AddScoped(typeof(IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>),
                        this.permissionManagementServiceType);
                }
            });
    }

    private TPermissionBindingInfo ApplyOptionalPaths<TPermissionBindingInfo>(TPermissionBindingInfo bindingInfo)
        where TPermissionBindingInfo : PermissionBindingInfo<TPermission, TPrincipal>
    {
        return bindingInfo
            .PipeMaybe(this.startDateAccessors, (b, v) => b with { PermissionStartDate = v })
            .PipeMaybe(this.endDatedAccessors, (b, v) => b with { PermissionEndDate = v })
            .PipeMaybe(this.commentPath, (b, v) => b with { PermissionComment = v.ToPropertyAccessors() })
            .PipeMaybe(this.delegatedFromPath, (b, v) => b with { DelegatedFrom = v.ToPropertyAccessors() })
            .PipeMaybe(this.isReadonly, (b, v) => b with { IsReadonly = v });
    }

    private TGeneralPermissionBindingInfo ApplyGeneralOptionalPaths<TGeneralPermissionBindingInfo>(TGeneralPermissionBindingInfo bindingInfo)
        where TGeneralPermissionBindingInfo : GeneralPermissionBindingInfo<TPermission, TSecurityRole>
    {
        return bindingInfo
            .PipeMaybe(this.descriptionPath, (b, v) => b with { SecurityRoleDescription = v.ToPropertyAccessors() });
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionPeriod(
        PropertyAccessors<TPermission, DateTime?>? startDatePropertyAccessor,
        PropertyAccessors<TPermission, DateTime?>? endDatePropertyAccessor)
    {
        this.startDateAccessors = startDatePropertyAccessor;
        this.endDatedAccessors = endDatePropertyAccessor;

        return this;
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionPeriod(
        Expression<Func<TPermission, DateTime?>>? startDatePath,
        Expression<Func<TPermission, DateTime?>>? endDatePath)
    {
        return this.SetPermissionPeriod(
            startDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(startDatePath),
            endDatePath == null ? null : new PropertyAccessors<TPermission, DateTime?>(endDatePath));
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionComment(
        Expression<Func<TPermission, string>> newCommentPath)
    {
        this.commentPath = newCommentPath;

        return this;
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionDelegation(
        Expression<Func<TPermission, TPermission?>> newDelegatedFromPath)
    {
        this.delegatedFromPath = newDelegatedFromPath;

        return this;
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetSecurityRoleDescription(
        Expression<Func<TSecurityRole, string>>? newDescriptionPath)
    {
        this.descriptionPath = newDescriptionPath;

        return this;
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetReadonly(bool value = true)
    {
        this.isReadonly = value;

        return this;
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionEqualityComparer<TComparer>()
        where TComparer : IPermissionEqualityComparer<TPermission, TPermissionRestriction>
    {
        this.permissionEqualityComparerType = typeof(TComparer);

        return this;
    }

    public IGeneralPermissionSettings<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction> SetPermissionManagementService<
        TPermissionManagementService>()
        where TPermissionManagementService : IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction>
    {
        this.permissionManagementServiceType = typeof(TPermissionManagementService);

        return this;
    }

    private ISecuritySystemSettings RegisterGeneralServices(ISecuritySystemSettings settings)
    {
        return settings
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