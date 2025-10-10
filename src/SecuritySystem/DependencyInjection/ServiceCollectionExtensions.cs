﻿using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.AvailableSecurity;
using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders.AccessorsBuilder;
using SecuritySystem.Builders.MaterializedBuilder;
using SecuritySystem.Credential;
using SecuritySystem.DependencyInjection.DomainSecurityServiceBuilder;
using SecuritySystem.DomainServices;
using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.ExternalSystem.SecurityContextStorage;
using SecuritySystem.PermissionOptimization;
using SecuritySystem.Providers;
using SecuritySystem.RelativeDomainPathInfo;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.SecurityRuleInfo;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.AncestorDenormalization;

namespace SecuritySystem.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDomainSecurityServices(
        this IServiceCollection services,
        Action<IDomainSecurityServiceRootBuilder> setupAction)
    {
        var builder = new DomainSecurityServiceRootBuilder();

        setupAction(builder);

        builder.Register(services);

        return services;
    }

    public static IServiceCollection AddSecuritySystem(this IServiceCollection services, Action<ISecuritySystemSettings> setupAction)
    {
        services.RegisterGeneralSecuritySystem();

        var settings = new SecuritySystemSettings();

        setupAction(settings);

        settings.Initialize(services);

        return services;
    }

    public static IServiceCollection AddRelativeDomainPath<TFrom, TTo>(
        this IServiceCollection services,
        Expression<Func<TFrom, TTo>> path,
        string? key = null)
    {
        var info = new SingleRelativeDomainPathInfo<TFrom, TTo>(path);

        if (key == null)
        {
            return services.AddSingleton<IRelativeDomainPathInfo<TFrom, TTo>>(info);
        }
        else
        {
            return services.AddKeyedSingleton<IRelativeDomainPathInfo<TFrom, TTo>>(key, info);
        }
    }

    public static IServiceCollection AddRelativeDomainPath<TFrom, TTo>(
        this IServiceCollection services,
        Expression<Func<TFrom, IEnumerable<TTo>>> path,
        string? key = null)
    {
        var info = new ManyRelativeDomainPathInfo<TFrom, TTo>(path);

        if (key == null)
        {
            return services.AddSingleton<IRelativeDomainPathInfo<TFrom, TTo>>(info);
        }
        else
        {
            return services.AddKeyedSingleton<IRelativeDomainPathInfo<TFrom, TTo>>(key, info);
        }
    }

    private static IServiceCollection RegisterGeneralSecuritySystem(this IServiceCollection services)
    {
        return services

            .AddScoped(typeof(IDenormalizedAncestorsService<,>), typeof(DenormalizedAncestorsService<,>))
            .AddScoped(typeof(IAncestorLinkExtractor<,>), typeof(AncestorLinkExtractor<,>))
            .AddScoped<IDenormalizedAncestorsServiceFactory, DenormalizedAncestorsServiceFactory>()

            .AddSingleton<IExpressionEvaluatorStorage>(_ => new ExpressionEvaluatorStorage(LambdaCompileMode.All))
            .AddSingleton<IRealTypeResolver, IdentityRealTypeResolver>()
            .AddScoped<IHierarchicalObjectExpanderFactory, HierarchicalObjectExpanderFactory>()

            .AddSingleton<SecurityAdministratorRuleFactory>()

            .AddSingleton(new IdentityPropertySourceSettings("Id"))
            .AddSingleton<IIdentityPropertySource, IdentityPropertySource>()
            .AddSingleton<IIdentityInfoSource, IdentityInfoSource>()

            .AddScoped<ISecurityContextStorage, SecurityContextStorage>()
            .AddScoped(typeof(LocalStorage<,>))

            .AddScoped<IUserCredentialNameResolver, RootUserCredentialNameResolver>()

            .AddScoped<IRootPrincipalSourceService, RootPrincipalSourceService>()
            .AddScoped<IPrincipalManagementService, FakePrincipalManagementService>()

            .AddSingleton<IClientSecurityRuleNameExtractor, ClientSecurityRuleNameExtractor>()
            .AddSingleton<IClientSecurityRuleInfoSource, RootClientSecurityRuleInfoSource>()
            .AddKeyedSingleton<IClientSecurityRuleInfoSource, DomainModeClientSecurityRuleInfoSource>(RootClientSecurityRuleInfoSource.ElementKey)
            .AddSingleton<IClientSecurityRuleResolver, ClientSecurityRuleResolver>()
            .AddSingleton<IDomainModeSecurityRuleResolver, DomainModeSecurityRuleResolver>()
            .AddSingleton<IDomainSecurityRoleExtractor, DomainSecurityRoleExtractor>()

            .AddSingleton<ISecurityRuleHeaderExpander, SecurityRuleHeaderExpander>()
            .AddSingleton<IClientSecurityRuleExpander, ClientSecurityRuleExpander>()
            .AddSingleton<ISecurityModeExpander, SecurityModeExpander>()
            .AddSingleton<ISecurityOperationExpander, SecurityOperationExpander>()
            .AddSingleton<ISecurityRoleExpander, SecurityRoleExpander>()
            .AddSingleton<IRoleFactorySecurityRuleExpander, RoleFactorySecurityRuleExpander>()
            .AddSingleton<ISecurityRuleExpander, RootSecurityRuleExpander>()
            .AddSingleton<ISecurityRoleSource, SecurityRoleSource>()
            .AddSingleton<ISecurityOperationInfoSource, SecurityOperationInfoSource>()
            .AddScoped<ISecurityContextSource, SecurityContextSource>()
            .AddSingleton<ISecurityContextInfoSource, SecurityContextInfoSource>()
            .AddSingleton<ISecurityRuleBasicOptimizer, SecurityRuleBasicOptimizer>()
            .AddSingleton<ISecurityRuleDeepOptimizer, SecurityRuleDeepOptimizer>()

            .AddScoped(typeof(IRoleBaseSecurityProviderFactory<>), typeof(RoleBaseSecurityProviderFactory<>))
            .AddScoped(typeof(IDomainSecurityProviderFactory<>), typeof(DomainSecurityProviderFactory<>))
            .AddSingleton<ISecurityPathRestrictionService, SecurityPathRestrictionService>()
            .AddScoped(typeof(ISecurityFilterFactory<>), typeof(SecurityFilterBuilderFactory<>))
            .AddScoped(typeof(IAccessorsFilterFactory<>), typeof(AccessorsFilterBuilderFactory<>))
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddKeyedScoped(
                typeof(ISecurityProvider<>),
                nameof(DomainSecurityRule.CurrentUser),
                typeof(CurrentUserSecurityProvider<>))
            .AddKeyedSingleton(
                typeof(ISecurityProvider<>),
                nameof(DomainSecurityRule.AccessDenied),
                typeof(AccessDeniedSecurityProvider<>))
            .AddKeyedSingleton(typeof(ISecurityProvider<>), nameof(SecurityRule.Disabled), typeof(DisabledSecurityProvider<>))
            .AddSingleton(typeof(ISecurityProvider<>), typeof(DisabledSecurityProvider<>))
            .AddScoped(typeof(IDomainSecurityService<>), typeof(ContextDomainSecurityService<>))

            .AddScoped<ISecuritySystemFactory, SecuritySystemFactory>()
            .AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<ISecuritySystemFactory>();
                var securityRuleCredential = sp.GetRequiredService<SecurityRuleCredential>();

                return factory.Create(securityRuleCredential);
            })

            .AddKeyedScoped(
                nameof(SecurityRuleCredential.CurrentUserWithoutRunAsCredential),
                (sp, _) => sp.GetRequiredService<ISecuritySystemFactory>()
                    .Create(new SecurityRuleCredential.CurrentUserWithoutRunAsCredential()))

            .AddScoped(sp =>
            {
                var factoryList = sp.GetRequiredService<IEnumerable<IPermissionSystemFactory>>();
                var securityRuleCredential = sp.GetRequiredService<SecurityRuleCredential>();

                return factoryList.Select(factory => factory.Create(securityRuleCredential));
            })

            .AddSingleton<ISecurityRolesIdentsResolver, SecurityRolesIdentsResolver>()

            .AddSingleton<IRuntimePermissionOptimizationService, RuntimePermissionOptimizationService>()

            .AddSingleton<ISecurityAccessorDataOptimizer, SecurityAccessorDataOptimizer>()
            .AddKeyedScoped<ISecurityAccessorResolver, RawSecurityAccessorResolver>(RawSecurityAccessorResolver.Key)
            .AddScoped<ISecurityAccessorResolver, RootSecurityAccessorResolver>()

            .AddScoped<IAvailableSecurityRoleSource, AvailableSecurityRoleSource>()
            .AddScoped<IAvailableSecurityOperationSource, AvailableSecurityOperationSource>()
            .AddScoped<IAvailableClientSecurityRuleSource, AvailableClientSecurityRuleSource>()
            .AddScoped<IUserNameResolver, UserNameResolver>();
    }
}