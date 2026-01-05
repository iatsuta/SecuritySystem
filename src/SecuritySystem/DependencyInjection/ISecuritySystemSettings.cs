using CommonFramework.DependencyInjection;
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

public interface ISecuritySystemSettings
{
    /// <summary>
    /// Автоматическое добавление относительных путей на самих себя (v => v)
    /// </summary>
    bool AutoAddSelfRelativePath { get; set; }

    bool InitializeDefaultRoles { get; set; }

    ISecuritySystemSettings SetSecurityAdministratorRule(DomainSecurityRule.RoleBaseSecurityRule rule);

    ISecuritySystemSettings AddSecurityContext<TSecurityContext>(TypedSecurityIdentity identity, Action<ISecurityContextInfoBuilder<TSecurityContext>>? setup = null)
        where TSecurityContext : class, ISecurityContext;

    ISecuritySystemSettings AddDomainSecurity<TDomainObject>(Action<IDomainSecurityServiceBuilder<TDomainObject>> setup);

    ISecuritySystemSettings AddDomainSecurity<TDomainObject>(DomainSecurityRule viewSecurityRule,
        SecurityPath<TDomainObject> securityPath)
    {
        return this.AddDomainSecurity(viewSecurityRule, null, securityPath);
    }

    ISecuritySystemSettings AddDomainSecurity<TDomainObject>(DomainSecurityRule viewSecurityRule,
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

    ISecuritySystemSettings AddDomainSecurityMetadata<TMetadata>()
        where TMetadata : IDomainSecurityServiceMetadata;

    ISecuritySystemSettings AddSecurityRole(SecurityRole securityRole, SecurityRoleInfo info);

    ISecuritySystemSettings AddSecurityRule(DomainSecurityRule.SecurityRuleHeader header, DomainSecurityRule implementation);

    ISecuritySystemSettings AddSecurityOperation(SecurityOperation securityOperation, SecurityOperationInfo info);

    ISecuritySystemSettings AddPermissionSystem<TPermissionSystemFactory>()
        where TPermissionSystemFactory : class, IPermissionSystemFactory;

    ISecuritySystemSettings AddPermissionSystem(Func<IServiceProvider, IPermissionSystemFactory> getFactory);

    ISecuritySystemSettings AddPermissionSystem(Func<IServiceProxyFactory, IPermissionSystemFactory> getFactory);

    ISecuritySystemSettings AddRunAsValidator<TValidator>()
        where TValidator : class, IRunAsValidator;

    ISecuritySystemSettings AddExtensions(ISecuritySystemExtension extensions);

    ISecuritySystemSettings AddExtensions(Action<IServiceCollection> addServicesAction) =>
        this.AddExtensions(new SecuritySystemExtension(addServicesAction));

    ISecuritySystemSettings SetAccessDeniedExceptionService<TAccessDeniedExceptionService>()
        where TAccessDeniedExceptionService : class, IAccessDeniedExceptionService;

    ISecuritySystemSettings AddUserSource<TUser>(Action<IUserSourceBuilder<TUser>>? setupUserSource = null)
	    where TUser : class;

    ISecuritySystemSettings SetSecurityAccessorInfinityStorage<TStorage>()
        where TStorage : class, ISecurityAccessorInfinityStorage;

    ISecuritySystemSettings SetPrincipalManagementService<TPrincipalManagementService>()
        where TPrincipalManagementService : class, IPrincipalManagementService;

    ISecuritySystemSettings SetDefaultSecurityRuleCredential(SecurityRuleCredential securityRuleCredential);

    ISecuritySystemSettings SetClientDomainModeSecurityRuleSource<TClientDomainModeSecurityRuleSource>()
        where TClientDomainModeSecurityRuleSource : class, IClientDomainModeSecurityRuleSource;

    ISecuritySystemSettings AddClientSecurityRuleInfoSource<TClientSecurityRuleInfoSource>()
        where TClientSecurityRuleInfoSource : class, IClientSecurityRuleInfoSource;

    ISecuritySystemSettings AddClientSecurityRuleInfoSource(Type sourceType);

    ISecuritySystemSettings SetQueryableSource<TQueryableSource>()
        where TQueryableSource : class, IQueryableSource;
    ISecuritySystemSettings SetGenericRepository<TGenericRepository>()
	    where TGenericRepository : class, IGenericRepository;

	ISecuritySystemSettings SetRawUserAuthenticationService<TRawUserAuthenticationService>(bool withImpersonate = true)
        where TRawUserAuthenticationService : class, IRawUserAuthenticationService;
}