using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSystemFactory<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    IServiceProvider serviceProvider,
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo)
    : IPermissionSystemFactory

    where TPrincipal : class
    where TPermission : class
    where TSecurityRole : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
{
    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential) =>
        ActivatorUtilities
            .CreateInstance<
                GeneralPermissionSystem<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>(
                serviceProvider, bindingInfo, securityRuleCredential);
}