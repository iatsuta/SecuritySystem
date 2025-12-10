using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSystemFactory(IServiceProvider serviceProvider) : IPermissionSystemFactory
{
    public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential)
    {
        throw new NotImplementedException();
    }
}

public class GeneralPermissionSystemFactory<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent,
	TSecurityContextTypeIdent>(IServiceProvider serviceProvider) : IPermissionSystemFactory

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
	where TSecurityContextTypeIdent : notnull
{
	public IPermissionSystem Create(SecurityRuleCredential securityRuleCredential) =>
		ActivatorUtilities
			.CreateInstance<
				GeneralPermissionSystem<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent,
					TSecurityContextTypeIdent>>(serviceProvider, securityRuleCredential);
}