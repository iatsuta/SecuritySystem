using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.DependencyInjection.DomainSecurityServiceBuilder;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.ApplicationSecurity;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.SecurityRuleInfo;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

using System.Linq.Expressions;

namespace SecuritySystem.DependencyInjection;

public class SecuritySystemSettings : ISecuritySystemSettings
{
    private DomainSecurityRule.RoleBaseSecurityRule securityAdministratorRule = SecurityRole.Administrator;

    private readonly List<Action<IServiceCollection>> registerActions = [];

    private Action<IServiceCollection> registerRunAsManagerAction = _ => { };

    private Action<IServiceCollection>? registerQueryableSourceAction;

    private Action<IServiceCollection>? registerRawUserAuthenticationServiceAction;

    private Action<IServiceCollection>? registerGenericRepositoryAction;

    private SecurityRuleCredential defaultSecurityRuleCredential = new SecurityRuleCredential.CurrentUserWithRunAsCredential();

    private Type accessDeniedExceptionServiceType = typeof(AccessDeniedExceptionService);

    private Type clientDomainModeSecurityRuleSource = typeof(ClientDomainModeSecurityRuleSource);

    private Type securityAccessorInfinityStorageType = typeof(FakeSecurityAccessorInfinityStorage);

    public bool InitializeDefaultRoles { get; set; } = true;

    public ISecuritySystemSettings SetSecurityAdministratorRule(DomainSecurityRule.RoleBaseSecurityRule rule)
    {
        this.securityAdministratorRule = rule;

        return this;
    }

    public ISecuritySystemSettings AddSecurityContext<TSecurityContext>(SecurityIdentity identity, Action<ISecurityContextInfoBuilder<TSecurityContext>>? setup = null)
        where TSecurityContext : ISecurityContext
    {
        this.registerActions.Add(sc =>
        {
            var builder = new SecurityContextInfoBuilder<TSecurityContext>(identity);

            setup?.Invoke(builder);

            builder.Register(sc);
        });

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
        this.registerActions.Add(sc => sc.AddScoped(getFactory));

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

    public ISecuritySystemSettings SetUserSource<TUser>(
        Expression<Func<TUser, string>> namePath,
        Expression<Func<TUser, bool>> filter,
        Expression<Func<TUser, TUser?>>? runAsPath = null)
        where TUser : class
    {
        this.registerActions.Add(sc =>
        {
	        var info = new UserSourceInfo<TUser>(namePath, filter);

	        sc.AddSingleton<UserSourceInfo>(info);
	        sc.AddSingleton(info);

	        sc.AddScoped<IUserCredentialNameByIdentityResolver, UserCredentialNameByIdentityResolver<TUser>>();
		});

        if (runAsPath != null)
        {
	        this.registerRunAsManagerAction = sc =>
	        {
		        if (this.registerGenericRepositoryAction == null)
		        {
			        throw new InvalidOperationException("GenericRepository must be initialized");
		        }


		        sc.AddScoped<IRunAsValidator, UserSourceRunAsValidator<TUser>>();

		        sc.AddSingleton(new UserSourceRunAsAccessorData<TUser>(runAsPath));
		        sc.AddSingleton<IUserSourceRunAsAccessor<TUser>, UserSourceRunAsAccessor<TUser>>();
		        sc.ReplaceScoped<IRunAsManager, RunAsManager<TUser>>();
	        };
        }

        return this;
    }

    public ISecuritySystemSettings SetRunAsManager<TRunAsManager>()
        where TRunAsManager : class, IRunAsManager
    {
        this.registerRunAsManagerAction = sc => sc.ReplaceScoped<IRunAsManager, TRunAsManager>();

        return this;
    }

    public ISecuritySystemSettings SetSecurityAccessorInfinityStorage<TStorage>()
        where TStorage : class, ISecurityAccessorInfinityStorage
    {
        this.securityAccessorInfinityStorageType = typeof(TStorage);

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
        this.registerActions.Add(
            sc => sc.AddKeyedSingleton<IClientSecurityRuleInfoSource>(RootClientSecurityRuleInfoSource.ElementKey,
                (sp, _) => ActivatorUtilities.CreateInstance<SourceTypeClientSecurityRuleInfoSource>(sp, sourceType)));

        return this;
    }

    public ISecuritySystemSettings SetQueryableSource<TQueryableSource>()
        where TQueryableSource : class, IQueryableSource
    {
        this.registerQueryableSourceAction = sc => sc.AddScoped<IQueryableSource, TQueryableSource>();

        return this;
    }

    public ISecuritySystemSettings SetQueryableSource(Func<IServiceProvider, IQueryableSource> selector)
    {
        this.registerQueryableSourceAction = sc => sc.AddScoped(selector);

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
        this.registerRawUserAuthenticationServiceAction = sc => sc.AddScoped(selector);

        return this;
    }

    public ISecuritySystemSettings SetGenericRepository<TGenericRepository>()
        where TGenericRepository : class, IGenericRepository
    {
        this.registerGenericRepositoryAction = sc => sc.AddScoped<IGenericRepository, TGenericRepository>();

        return this;
    }

    public ISecuritySystemSettings SetGenericRepository(Func<IServiceProvider, IGenericRepository> selector)
    {
        this.registerGenericRepositoryAction = sc => sc.AddScoped(selector);

        return this;
    }

    public void Initialize(IServiceCollection services)
    {
        (this.registerQueryableSourceAction ?? throw new InvalidOperationException("QueryableSource must be initialized")).Invoke(services);

        (this.registerRawUserAuthenticationServiceAction ?? throw new InvalidOperationException("RawUserAuthenticationService must be initialized")).Invoke(services);

        this.registerGenericRepositoryAction?.Invoke(services);

        services.AddSingleton(new SecurityAdministratorRuleInfo(this.securityAdministratorRule));

        this.registerActions.ForEach(v => v(services));

        this.registerRunAsManagerAction(services);

        if (this.InitializeDefaultRoles)
        {
            services.AddSingleton<IInitializedSecurityRoleSource, InitializedSecurityRoleSource>();
            services.AddSingletonFrom((IInitializedSecurityRoleSource source) => source.GetSecurityRoles());
        }

        services.AddSingleton(typeof(IAccessDeniedExceptionService), this.accessDeniedExceptionServiceType);

        services.AddScoped(typeof(ISecurityAccessorInfinityStorage), this.securityAccessorInfinityStorageType);

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
