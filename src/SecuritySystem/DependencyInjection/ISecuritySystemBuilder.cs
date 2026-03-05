using CommonFramework;
using CommonFramework.GenericRepository;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.AccessDenied;
using SecuritySystem.DependencyInjection.Domain;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.SecurityAccessor;
using SecuritySystem.SecurityRuleInfo;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.DependencyInjection;

public interface ISecuritySystemBuilder
{
    /// <summary>
    /// Автоматическое добавление относительных путей на самих себя (v => v)
    /// </summary>
    bool AutoAddSelfRelativePath { get; set; }

    bool InitializeDefaultRoles { get; set; }

    ISecuritySystemBuilder SetSecurityAdministratorRule(DomainSecurityRule.RoleBaseSecurityRule rule);

    ISecuritySystemBuilder AddSecurityContext<TSecurityContext>(TypedSecurityIdentity identity, Action<ISecurityContextInfoBuilder<TSecurityContext>>? setup = null)
        where TSecurityContext : class, ISecurityContext;

    ISecuritySystemBuilder AddDomainSecurity<TDomainObject>(Action<IDomainSecurityServiceBuilder<TDomainObject>> setup);

    ISecuritySystemBuilder AddDomainSecurity<TDomainObject>(DomainSecurityRule viewSecurityRule,
        SecurityPath<TDomainObject> securityPath)
    {
        return this.AddDomainSecurity(viewSecurityRule, null, securityPath);
    }

    ISecuritySystemBuilder AddDomainSecurity<TDomainObject>(DomainSecurityRule viewSecurityRule,
        DomainSecurityRule? editSecurityRule = null,
        SecurityPath<TDomainObject>? securityPath = null)
    {
        return this.AddDomainSecurity<TDomainObject>(
            b =>
            {
                b.SetView(viewSecurityRule);

                if (editSecurityRule != null)
                {
                    b.SetEdit(editSecurityRule);
                }

                if (securityPath != null)
                {
                    b.SetPath(securityPath);
                }
            });
    }

    ISecuritySystemBuilder AddDomainSecurityMetadata<TMetadata>()
        where TMetadata : IDomainSecurityServiceMetadata;

    ISecuritySystemBuilder AddSecurityRole(SecurityRole securityRole, SecurityRoleInfo info);

    ISecuritySystemBuilder AddSecurityRule(DomainSecurityRule.SecurityRuleHeader header, DomainSecurityRule implementation);

    ISecuritySystemBuilder AddSecurityOperation(SecurityOperation securityOperation, SecurityOperationInfo info);

    ISecuritySystemBuilder AddPermissionSystem<TPermissionSystemFactory>()
        where TPermissionSystemFactory : class, IPermissionSystemFactory;

    ISecuritySystemBuilder AddPermissionSystem(Func<IServiceProvider, IPermissionSystemFactory> getFactory);

    ISecuritySystemBuilder AddPermissionSystem(Func<IServiceProxyFactory, IPermissionSystemFactory> getFactory);

    ISecuritySystemBuilder AddRunAsValidator<TValidator>()
        where TValidator : class, IRunAsValidator;

    ISecuritySystemBuilder AddExtensions(ISecuritySystemExtension extensions);

    ISecuritySystemBuilder AddExtensions(Action<IServiceCollection> addServicesAction) =>
        this.AddExtensions(new SecuritySystemExtension(addServicesAction));

    ISecuritySystemBuilder SetAccessDeniedExceptionService<TAccessDeniedExceptionService>()
        where TAccessDeniedExceptionService : class, IAccessDeniedExceptionService;

    ISecuritySystemBuilder AddUserSource<TUser>(Action<IUserSourceBuilder<TUser>>? setupUserSource = null)
	    where TUser : class;

    ISecuritySystemBuilder SetSecurityAccessorInfinityStorage<TStorage>()
        where TStorage : class, ISecurityAccessorInfinityStorage;

    ISecuritySystemBuilder SetPrincipalManagementService<TPrincipalManagementService>()
        where TPrincipalManagementService : class, IPrincipalManagementService;

    ISecuritySystemBuilder SetDefaultSecurityRuleCredential(SecurityRuleCredential securityRuleCredential);

    ISecuritySystemBuilder SetClientDomainModeSecurityRuleSource<TClientDomainModeSecurityRuleSource>()
        where TClientDomainModeSecurityRuleSource : class, IClientDomainModeSecurityRuleSource;

    ISecuritySystemBuilder AddClientSecurityRuleInfoSource<TClientSecurityRuleInfoSource>()
        where TClientSecurityRuleInfoSource : class, IClientSecurityRuleInfoSource;

    ISecuritySystemBuilder AddClientSecurityRuleInfoSource(Type sourceType);

    ISecuritySystemBuilder SetQueryableSource<TQueryableSource>()
        where TQueryableSource : class, IQueryableSource;
    ISecuritySystemBuilder SetGenericRepository<TGenericRepository>()
	    where TGenericRepository : class, IGenericRepository;

	ISecuritySystemBuilder SetRawUserAuthenticationService<TRawUserAuthenticationService>(bool withImpersonate = true)
        where TRawUserAuthenticationService : class, IRawUserAuthenticationService;
}