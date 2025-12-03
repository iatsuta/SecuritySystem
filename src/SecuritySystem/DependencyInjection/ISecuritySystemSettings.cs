using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DependencyInjection.DomainSecurityServiceBuilder;
using SecuritySystem.ExternalSystem;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.SecurityRuleInfo;
using SecuritySystem.Services;

namespace SecuritySystem.DependencyInjection;

public interface ISecuritySystemSettings
{
    bool InitializeDefaultRoles { get; set; }

    ISecuritySystemSettings SetSecurityAdministratorRule(DomainSecurityRule.RoleBaseSecurityRule rule);

    ISecuritySystemSettings AddSecurityContext<TSecurityContext>(SecurityIdentity identity, Action<ISecurityContextInfoBuilder<TSecurityContext>>? setup = null)
        where TSecurityContext : class, ISecurityContext;

    ISecuritySystemSettings AddDomainSecurityServices(Action<IDomainSecurityServiceRootBuilder> setup);

    ISecuritySystemSettings AddSecurityRole(SecurityRole securityRole, SecurityRoleInfo info);

    ISecuritySystemSettings AddSecurityRule(DomainSecurityRule.SecurityRuleHeader header, DomainSecurityRule implementation);

    ISecuritySystemSettings AddSecurityOperation(SecurityOperation securityOperation, SecurityOperationInfo info);

    ISecuritySystemSettings AddPermissionSystem<TPermissionSystemFactory>()
        where TPermissionSystemFactory : class, IPermissionSystemFactory;

    ISecuritySystemSettings AddPermissionSystem(Func<IServiceProvider, IPermissionSystemFactory> getFactory);

    ISecuritySystemSettings AddExtensions(ISecuritySystemExtension extensions);

    ISecuritySystemSettings AddExtensions(Action<IServiceCollection> addServicesAction) =>
        this.AddExtensions(new SecuritySystemExtension(addServicesAction));

    ISecuritySystemSettings SetAccessDeniedExceptionService<TAccessDeniedExceptionService>()
        where TAccessDeniedExceptionService : class, IAccessDeniedExceptionService;

    ISecuritySystemSettings AddUserSource<TUser>(Action<IUserSourceBuilder<TUser>> setupUserSource)
	    where TUser : class;

    ISecuritySystemSettings SetSecurityAccessorInfinityStorage<TStorage>()
        where TStorage : class, ISecurityAccessorInfinityStorage;

    ISecuritySystemSettings SetDefaultSecurityRuleCredential(SecurityRuleCredential securityRuleCredential);

    ISecuritySystemSettings SetClientDomainModeSecurityRuleSource<TClientDomainModeSecurityRuleSource>()
        where TClientDomainModeSecurityRuleSource : class, IClientDomainModeSecurityRuleSource;

    ISecuritySystemSettings AddClientSecurityRuleInfoSource<TClientSecurityRuleInfoSource>()
        where TClientSecurityRuleInfoSource : class, IClientSecurityRuleInfoSource;

    ISecuritySystemSettings AddClientSecurityRuleInfoSource(Type sourceType);

    ISecuritySystemSettings SetQueryableSource<TQueryableSource>()
        where TQueryableSource : class, IQueryableSource;

    ISecuritySystemSettings SetQueryableSource(Func<IServiceProvider, IQueryableSource> selector);

    ISecuritySystemSettings SetRawUserAuthenticationService<TRawUserAuthenticationService>()
        where TRawUserAuthenticationService : class, IRawUserAuthenticationService;

    ISecuritySystemSettings SetRawUserAuthenticationService(Func<IServiceProvider, IRawUserAuthenticationService> selector);

    ISecuritySystemSettings SetGenericRepository<TGenericRepository>()
        where TGenericRepository : class, IGenericRepository;

    ISecuritySystemSettings SetGenericRepository(Func<IServiceProvider, IGenericRepository> selector);
}