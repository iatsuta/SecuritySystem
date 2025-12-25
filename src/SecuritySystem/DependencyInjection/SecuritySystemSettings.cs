using CommonFramework.DependencyInjection;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource.DependencyInjection;
using CommonFramework.VisualIdentitySource.DependencyInjection;

using HierarchicalExpand.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.AccessDenied;
using SecuritySystem.DependencyInjection.DomainSecurityServiceBuilder;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.SecurityRuleInfo;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.DependencyInjection;

public class SecuritySystemSettings : ISecuritySystemSettings
{
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




    public readonly List<Action<IIdentitySourceSettings>> IdentitySetupActions = new();

    public readonly List<Action<IVisualIdentitySourceSettings>> VisualIdentitySetupActions = new();

    public readonly List<Action<IHierarchicalExpandSettings>> HierarchicalSetupActions = new();


	public bool InitializeDefaultRoles { get; set; } = true;

    public ISecuritySystemSettings SetSecurityAdministratorRule(DomainSecurityRule.RoleBaseSecurityRule rule)
    {
        this.securityAdministratorRule = rule;

        return this;
    }

    public ISecuritySystemSettings AddSecurityContext<TSecurityContext>(TypedSecurityIdentity identity, Action<ISecurityContextInfoBuilder<TSecurityContext>>? setup = null)
        where TSecurityContext : class, ISecurityContext
    {
	    var builder = new SecurityContextInfoBuilder<TSecurityContext>(identity);

	    setup?.Invoke(builder);

	    if (builder.IdentitySetupAction != null)
	    {
            this.IdentitySetupActions.Add(builder.IdentitySetupAction);
	    }

	    if (builder.VisualIdentitySetupAction != null)
	    {
		    this.VisualIdentitySetupActions.Add(builder.VisualIdentitySetupAction);
	    }

	    if (builder.HierarchicalSetupAction != null)
	    {
		    this.HierarchicalSetupActions.Add(builder.HierarchicalSetupAction);
	    }

		this.registerActions.Add(sc => builder.Register(sc));

        return this;
    }

    public ISecuritySystemSettings AddDomainSecurityServices(Action<IDomainSecurityServiceRootBuilder> setup)
    {
        this.registerActions.Add(sc => sc.RegisterDomainSecurityServices(setup));

        return this;
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
		    throw new Exception($"{nameof(UserSource<>)} for {typeof(TUser).Name} already initialized");
	    }

	    var userSourceBuilder = new UserSourceBuilder<TUser>();

	    setupUserSource?.Invoke(userSourceBuilder);

	    if (userSourceBuilder.VisualIdentitySetupAction != null)
	    {
		    this.VisualIdentitySetupActions.Add(userSourceBuilder.VisualIdentitySetupAction);
	    }

		this.registerActions.Add(sc =>
	    {
			var info = new UserSourceInfo<TUser>(userSourceBuilder.FilterPath);

		    sc.AddSingleton<UserSourceInfo>(info);
		    sc.AddSingleton(info);

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

                    sc.AddScopedFrom<IUserSource, IUserSource<TUser>>();
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
        this.registerActions.Add(sc => sc.AddKeyedSingleton<IClientSecurityRuleInfoSource, TClientSecurityRuleInfoSource>(RootClientSecurityRuleInfoSource.ElementKey));

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

    public ISecuritySystemSettings SetRawUserAuthenticationService<TRawUserAuthenticationService>()
        where TRawUserAuthenticationService : class, IRawUserAuthenticationService
    {
        this.registerRawUserAuthenticationServiceAction = sc => sc.AddScoped<IRawUserAuthenticationService, TRawUserAuthenticationService>();

        return this;
    }

    public ISecuritySystemSettings SetRawUserAuthenticationService(Func<IServiceProvider, IRawUserAuthenticationService> selector)
    {
        this.registerRawUserAuthenticationServiceAction = sc => sc.AddScopedFrom(selector);

        return this;
    }

    public ISecuritySystemSettings SetRawUserAuthenticationService(Func<IServiceProxyFactory, IRawUserAuthenticationService> selector)
    {
        this.registerRawUserAuthenticationServiceAction = sc => sc.AddScopedFrom(selector);

        return this;
    }

    public ISecuritySystemSettings SetGenericRepository<TGenericRepository>()
        where TGenericRepository : class, IGenericRepository
    {
        this.registerGenericRepositoryAction = sc => sc.AddScoped<IGenericRepository, TGenericRepository>();

        return this;
    }

    public void Initialize(IServiceCollection services)
    {
        (this.registerQueryableSourceAction ?? throw new InvalidOperationException("QueryableSource must be initialized")).Invoke(services);

        (this.registerGenericRepositoryAction ?? throw new InvalidOperationException("GenericRepository must be initialized")).Invoke(services);

		(this.registerRawUserAuthenticationServiceAction ?? throw new InvalidOperationException("RawUserAuthenticationService must be initialized")).Invoke(services);

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
}
