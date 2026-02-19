using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource.DependencyInjection;
using CommonFramework.RelativePath;
using CommonFramework.VisualIdentitySource.DependencyInjection;
using HierarchicalExpand.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.AccessDenied;
using SecuritySystem.AvailableSecurity;
using SecuritySystem.Builders._Factory;
using SecuritySystem.Builders.AccessorsBuilder;
using SecuritySystem.Credential;
using SecuritySystem.DependencyInjection.Domain;
using SecuritySystem.DomainServices;
using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.ExternalSystem.SecurityContextStorage;
using SecuritySystem.PermissionOptimization;
using SecuritySystem.Providers;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.SecurityRuleInfo;
using SecuritySystem.Services;
using SecuritySystem.UserSource;
using System.Globalization;
using System.Reflection;
using SecuritySystem.Builders.MaterializedBuilder;

namespace SecuritySystem.DependencyInjection;

public class SecuritySystemSettings : ISecuritySystemSettings
{
    private readonly List<DomainSecurityServiceBuilder> domainBuilders = [];

    private readonly HashSet<Type> userSourceTypes = new();

    private DomainSecurityRule.RoleBaseSecurityRule securityAdministratorRule = SecurityRole.Administrator;

    private readonly List<Action<IServiceCollection>> registerActions = [];

    private Action<IServiceCollection>? registerRunAsManagerAction;

    private Action<IServiceCollection>? registerQueryableSourceAction;

    private Action<IServiceCollection>? registerRawUserAuthenticationServiceAction;

    private Action<IServiceCollection>? registerGenericRepositoryAction;

    private SecurityRuleCredential defaultSecurityRuleCredential = new SecurityRuleCredential.CurrentUserWithRunAsCredential();

    private Type accessDeniedExceptionServiceType = typeof(AccessDeniedExceptionService);

    private Type clientDomainModeSecurityRuleSource = typeof(ClientDomainModeSecurityRuleSource);

    private Type securityAccessorInfinityStorageType = typeof(FakeSecurityAccessorInfinityStorage);

    private Type principalManagementServiceType = typeof(FakePrincipalManagementService);


    private readonly List<Action<IIdentitySourceSettings>> identitySetupActions = [];

    private readonly List<Action<IVisualIdentitySourceSettings>> visualIdentitySetupActions = [];

    private readonly List<Action<IHierarchicalExpandSettings>> hierarchicalSetupActions = [];


    public bool InitializeDefaultRoles { get; set; } = true;

    public bool AutoAddSelfRelativePath { get; set; } = true;

    public ISecuritySystemSettings SetSecurityAdministratorRule(DomainSecurityRule.RoleBaseSecurityRule rule)
    {
        this.securityAdministratorRule = rule;

        return this;
    }

    public ISecuritySystemSettings AddSecurityContext<TSecurityContext>(TypedSecurityIdentity identity,
        Action<ISecurityContextInfoBuilder<TSecurityContext>>? setup = null)
        where TSecurityContext : class, ISecurityContext
    {
        var builder = new SecurityContextInfoBuilder<TSecurityContext>(identity);

        setup?.Invoke(builder);

        if (builder.IdentitySetupAction != null)
        {
            this.identitySetupActions.Add(builder.IdentitySetupAction);
        }

        if (builder.VisualIdentitySetupAction != null)
        {
            this.visualIdentitySetupActions.Add(builder.VisualIdentitySetupAction);
        }

        if (builder.HierarchicalSetupAction != null)
        {
            this.hierarchicalSetupActions.Add(builder.HierarchicalSetupAction);
        }

        this.registerActions.Add(sc => builder.Register(sc));

        return this;
    }

    public ISecuritySystemSettings AddDomainSecurity<TDomainObject>(Action<IDomainSecurityServiceBuilder<TDomainObject>> setup)
    {
        var builder = new DomainSecurityServiceBuilder<TDomainObject>();

        setup(builder);

        this.domainBuilders.Add(builder);

        return this;
    }

    public ISecuritySystemSettings AddDomainSecurityMetadata<TMetadata>()
        where TMetadata : IDomainSecurityServiceMetadata
    {
        return this.GetType().GetMethod(nameof(this.AddDomainSecurityMetadataInternal), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(typeof(TMetadata), TMetadata.DomainType)
            .Invoke<ISecuritySystemSettings>(this);
    }

    private ISecuritySystemSettings AddDomainSecurityMetadataInternal<TMetadata, TDomainObject>()
        where TMetadata : IDomainSecurityServiceMetadata<TDomainObject>
    {
        return this.AddDomainSecurity<TDomainObject>(b => b.Override<TMetadata>().Pipe(TMetadata.Setup));
    }



    public ISecuritySystemSettings AddSecurityRole(SecurityRole securityRole, SecurityRoleInfo info)
    {
        this.registerActions.Add(sc => this.AddSecurityRole(sc, new FullSecurityRole(securityRole.Name, info)));

        return this;
    }

    public ISecuritySystemSettings AddSecurityRule(DomainSecurityRule.SecurityRuleHeader header, DomainSecurityRule implementation)
    {
        this.registerActions.Add(sc => sc.AddSingleton(new SecurityRuleHeaderInfo(header, implementation)));

        return this;
    }

    public ISecuritySystemSettings AddSecurityOperation(SecurityOperation securityOperation, SecurityOperationInfo info)
    {
        this.registerActions.Add(sc => sc.AddSingleton(new FullSecurityOperation(securityOperation, info)));

        return this;
    }

    public ISecuritySystemSettings AddPermissionSystem<TPermissionSystemFactory>()
        where TPermissionSystemFactory : class, IPermissionSystemFactory
    {
        this.registerActions.Add(sc => sc.AddScoped<IPermissionSystemFactory, TPermissionSystemFactory>());

        return this;
    }

    public ISecuritySystemSettings AddPermissionSystem(Func<IServiceProvider, IPermissionSystemFactory> getFactory)
    {
        this.registerActions.Add(sc => sc.AddScopedFrom(getFactory));

        return this;
    }

    public ISecuritySystemSettings AddPermissionSystem(Func<IServiceProxyFactory, IPermissionSystemFactory> getFactory)
    {
        this.registerActions.Add(sc => sc.AddScopedFrom(getFactory));

        return this;
    }

    public ISecuritySystemSettings AddRunAsValidator<TValidator>()
        where TValidator : class, IRunAsValidator
    {
        this.registerActions.Add(sc => sc.AddScoped<TValidator>());

        return this;
    }

    public ISecuritySystemSettings AddExtensions(ISecuritySystemExtension extensions)
    {
        this.registerActions.Add(extensions.AddServices);

        return this;
    }

    public ISecuritySystemSettings SetAccessDeniedExceptionService<TAccessDeniedExceptionService>()
        where TAccessDeniedExceptionService : class, IAccessDeniedExceptionService
    {
        this.accessDeniedExceptionServiceType = typeof(TAccessDeniedExceptionService);

        return this;
    }

    public ISecuritySystemSettings AddUserSource<TUser>(Action<IUserSourceBuilder<TUser>>? setupUserSource)
        where TUser : class
    {
        if (!userSourceTypes.Add(typeof(TUser)))
        {
            throw new InvalidOperationException($"{nameof(UserSource<>)} for {typeof(TUser).Name} already initialized");
        }

        var userSourceBuilder = new UserSourceBuilder<TUser>();

        setupUserSource?.Invoke(userSourceBuilder);

        if (userSourceBuilder.VisualIdentitySetupAction != null)
        {
            this.visualIdentitySetupActions.Add(userSourceBuilder.VisualIdentitySetupAction);
        }

        this.registerActions.Add(sc =>
        {
            var info = new UserSourceInfo<TUser>(userSourceBuilder.FilterPath);

            sc.AddSingleton<UserSourceInfo>(info);
            sc.AddSingleton(info);

            sc.AddScopedFrom<IUserSource, IUserSource<TUser>>();

            sc.AddScoped(typeof(IMissedUserService<TUser>), userSourceBuilder.MissedUserServiceType);
        });

        if (userSourceBuilder.RunAsPath != null)
        {
            if (this.registerRunAsManagerAction == null)
            {
                this.registerRunAsManagerAction = sc =>
                {
                    sc.AddSingleton(new UserSourceRunAsInfo<TUser>(userSourceBuilder.RunAsPath));
                    sc.AddScoped<IRunAsManager, RunAsManager<TUser>>();
                };
            }
            else
            {
                throw new InvalidOperationException("RunAs already initialized");
            }
        }

        return this;
    }

    public ISecuritySystemSettings SetSecurityAccessorInfinityStorage<TStorage>()
        where TStorage : class, ISecurityAccessorInfinityStorage
    {
        this.securityAccessorInfinityStorageType = typeof(TStorage);

        return this;
    }

    public ISecuritySystemSettings SetPrincipalManagementService<TPrincipalManagementService>()
        where TPrincipalManagementService : class, IPrincipalManagementService
    {
        this.principalManagementServiceType = typeof(TPrincipalManagementService);

        return this;
    }

    public ISecuritySystemSettings SetDefaultSecurityRuleCredential(SecurityRuleCredential securityRuleCredential)
    {
        this.defaultSecurityRuleCredential = securityRuleCredential;

        return this;
    }

    public ISecuritySystemSettings SetClientDomainModeSecurityRuleSource<TClientDomainModeSecurityRuleSource>()
        where TClientDomainModeSecurityRuleSource : class, IClientDomainModeSecurityRuleSource
    {
        this.clientDomainModeSecurityRuleSource = typeof(TClientDomainModeSecurityRuleSource);

        return this;
    }

    public ISecuritySystemSettings AddClientSecurityRuleInfoSource<TClientSecurityRuleInfoSource>()
        where TClientSecurityRuleInfoSource : class, IClientSecurityRuleInfoSource
    {
        this.registerActions.Add(sc =>
            sc.AddKeyedSingleton<IClientSecurityRuleInfoSource, TClientSecurityRuleInfoSource>(RootClientSecurityRuleInfoSource.ElementKey));

        return this;
    }

    public ISecuritySystemSettings AddClientSecurityRuleInfoSource(Type sourceType)
    {
        this.registerActions.Add(sc => sc.AddKeyedSingleton<IClientSecurityRuleInfoSource>(RootClientSecurityRuleInfoSource.ElementKey,
            (sp, _) => sp.GetRequiredService<IServiceProxyFactory>()
                .Create<IClientSecurityRuleInfoSource, SourceTypeClientSecurityRuleInfoSource>(sourceType)));

        return this;
    }

    public ISecuritySystemSettings SetQueryableSource<TQueryableSource>()
        where TQueryableSource : class, IQueryableSource
    {
        this.registerQueryableSourceAction = sc => sc.AddScoped<IQueryableSource, TQueryableSource>();

        return this;
    }

    public ISecuritySystemSettings SetRawUserAuthenticationService<TRawUserAuthenticationService>(bool withImpersonate = true)
        where TRawUserAuthenticationService : class, IRawUserAuthenticationService
    {
        this.registerRawUserAuthenticationServiceAction = sc =>
        {
            sc.AddScoped<TRawUserAuthenticationService>();
            sc.AddScopedFrom<IRawUserAuthenticationService, TRawUserAuthenticationService>();

            if (withImpersonate && typeof(IImpersonateService).IsAssignableFrom(typeof(TRawUserAuthenticationService)))
            {
                sc.AddScopedFrom<IImpersonateService>(sp => (IImpersonateService)sp.GetRequiredService(typeof(TRawUserAuthenticationService)));
            }
        };

        return this;
    }

    public ISecuritySystemSettings SetGenericRepository<TGenericRepository>()
        where TGenericRepository : class, IGenericRepository
    {
        this.registerGenericRepositoryAction = sc => sc.AddScoped<IGenericRepository, TGenericRepository>();

        return this;
    }

    private void AddSecurityRole(IServiceCollection serviceCollection, FullSecurityRole fullSecurityRole)
    {
        if (this.InitializeDefaultRoles)
        {
            serviceCollection.AddSingleton(new PreInitializerFullSecurityRole(fullSecurityRole));
        }
        else
        {
            serviceCollection.AddSingleton(fullSecurityRole);
        }
    }

    public void Initialize(IServiceCollection services)
    {
        this.RegisterGeneralServices(services);

        services.AddIdentitySource(s => this.identitySetupActions.Foreach(action => action(s)));
        services.AddVisualIdentitySource(s => this.visualIdentitySetupActions.ForEach(action => action(s)));
        services.AddHierarchicalExpand(s => this.hierarchicalSetupActions.ForEach(action => action(s)));

        (this.registerQueryableSourceAction ?? throw new InvalidOperationException("QueryableSource must be initialized")).Invoke(services);

        (this.registerGenericRepositoryAction ?? throw new InvalidOperationException("GenericRepository must be initialized")).Invoke(services);

        (this.registerRawUserAuthenticationServiceAction ?? throw new InvalidOperationException("RawUserAuthenticationService must be initialized"))
            .Invoke(services);

        services.AddSingleton(new SecurityAdministratorRuleInfo(this.securityAdministratorRule));

        this.registerActions.ForEach(v => v(services));

        if (this.registerRunAsManagerAction != null)
        {
            this.registerRunAsManagerAction(services);
        }

        if (this.InitializeDefaultRoles)
        {
            services.AddSingleton<IInitializedSecurityRoleSource, InitializedSecurityRoleSource>();
            services.AddSingletonFrom((IInitializedSecurityRoleSource source) => source.GetSecurityRoles());
        }

        services.AddSingleton(typeof(IAccessDeniedExceptionService), this.accessDeniedExceptionServiceType);

        services.AddScoped(typeof(ISecurityAccessorInfinityStorage), this.securityAccessorInfinityStorageType);

        services.AddScoped(typeof(IPrincipalManagementService), this.principalManagementServiceType);

        services.AddSingleton(this.defaultSecurityRuleCredential);

        services.AddSingleton(typeof(IClientDomainModeSecurityRuleSource), this.clientDomainModeSecurityRuleSource);

        foreach (var domainBuilder in this.domainBuilders)
        {
            domainBuilder.Register(services);

            if (this.AutoAddSelfRelativePath)
            {
                services.AddSingleton(
                    typeof(IRelativeDomainPathInfo<,>).MakeGenericType(domainBuilder.DomainType, domainBuilder.DomainType),
                    typeof(SelfRelativeDomainPathInfo<>).MakeGenericType(domainBuilder.DomainType));
            }
        }
    }




    private IServiceCollection RegisterGeneralServices(IServiceCollection services)
    {
        return services

            .AddScoped<IUserCredentialNameResolver, UserCredentialNameResolver>()

            .AddSingleton(typeof(IManagedPrincipalHeaderConverterFactory<>), typeof(ManagedPrincipalHeaderConverterFactory<>))
            .AddSingleton(typeof(IManagedPrincipalHeaderConverter<>), typeof(ManagedPrincipalHeaderConverter<>))
            .AddScoped(typeof(IPrincipalDomainService<>), typeof(PrincipalDomainService<>))
            .AddScoped(typeof(IAvailablePermissionFilterFactory<>), typeof(AvailablePermissionFilterFactory<>))
            .AddScoped(typeof(IAvailablePermissionSource<>), typeof(AvailablePermissionSource<>))
            .AddScoped(typeof(IAvailablePrincipalSource<>), typeof(AvailablePrincipalSource<>))
            .AddSingleton<IPermissionBindingInfoSource, PermissionBindingInfoSource>()

            .AddSingleton(typeof(ISecurityIdentityExtractor<>), typeof(SecurityIdentityExtractor<>))
            .AddSingleton(typeof(ISecurityIdentityConverter<>), typeof(SecurityIdentityConverter<>))
            .AddSingleton<IPrincipalDataSecurityIdentityExtractor, PrincipalDataSecurityIdentityExtractor>()

            .AddSingleton<IMissedUserErrorSource, MissedUserErrorSource>()

            .AddSingleton<IFormatProviderSource>(new FormatProviderSource(CultureInfo.CurrentCulture))
            .AddSingleton(typeof(IIdentsParser<>), typeof(IdentsParser<>))
            .AddSingleton<IDomainObjectIdentsParser, DomainObjectIdentsParser>()

            .AddScoped(typeof(ISecurityRepository<>), typeof(SecurityRepository<>))
            .AddSingleton(typeof(ISecurityIdentityFilterFactory<>), typeof(SecurityIdentityFilterFactory<>))

            .AddSingleton<IExpressionEvaluatorStorage>(_ => new ExpressionEvaluatorStorage(LambdaCompileMode.All))

            .AddSingleton(typeof(IDefaultUserConverter<>), typeof(DefaultUserConverter<>))
            .AddScoped(typeof(ICurrentUserSource<>), typeof(CurrentUserSource<>))
            .AddScoped(typeof(IUserSource<>), typeof(UserSource<>))
            .AddScoped(typeof(IUserQueryableSource<>), typeof(UserQueryableSource<>))
            .AddScoped(typeof(IUserNameResolver<>), typeof(UserNameResolver<>))
            .AddScoped(typeof(IUserFilterFactory<>), typeof(UserFilterFactory<>))
            .AddKeyedScoped(typeof(ICurrentUserSource<>), nameof(SecurityRuleCredential.CurrentUserWithoutRunAsCredential), typeof(RawCurrentUserSource<>))
            .AddSingleton<SecurityAdministratorRuleFactory>()

            .AddSingleton(typeof(IUserCredentialMatcher<>), typeof(UserCredentialMatcher<>))

            .AddScoped<ISecurityContextStorage, SecurityContextStorage>()
            .AddScoped(typeof(LocalStorage<,>))

            .AddScoped<IRootPrincipalSourceService, RootPrincipalSourceService>()

            .AddSingleton<IClientSecurityRuleNameExtractor, ClientSecurityRuleNameExtractor>()
            .AddSingleton<IClientSecurityRuleInfoSource, RootClientSecurityRuleInfoSource>()
            .AddKeyedSingleton<IClientSecurityRuleInfoSource, DomainModeClientSecurityRuleInfoSource>(RootClientSecurityRuleInfoSource.ElementKey)
            .AddSingleton<IClientSecurityRuleResolver, ClientSecurityRuleResolver>()
            .AddSingleton<IDomainModeSecurityRuleResolver, DomainModeSecurityRuleResolver>()
            .AddSingleton<IDomainSecurityRoleExtractor, DomainSecurityRoleExtractor>()

            .AddSingleton<IExpandedRoleGroupSecurityRuleSetOptimizer, ExpandedRoleGroupSecurityRuleSetOptimizer>()

            .AddSingleton<ISecurityRuleHeaderExpander, SecurityRuleHeaderExpander>()
            .AddSingleton<IClientSecurityRuleExpander, ClientSecurityRuleExpander>()
            .AddSingleton<ISecurityModeExpander, SecurityModeExpander>()
            .AddSingleton<ISecurityOperationExpander, SecurityOperationExpander>()
            .AddSingleton<ISecurityRoleGroupExpander, SecurityRoleGroupExpander>()
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
            .AddKeyedScoped<ICurrentUser, RawCurrentUser>(nameof(SecurityRuleCredential.CurrentUserWithoutRunAsCredential))
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
            .AddScoped<IAvailableClientSecurityRuleSource, AvailableClientSecurityRuleSource>();
    }
}