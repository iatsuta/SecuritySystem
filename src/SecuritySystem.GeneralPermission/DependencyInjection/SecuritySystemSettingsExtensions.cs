using SecuritySystem.DependencyInjection;

namespace SecuritySystem.GeneralPermission.DependencyInjection;

public static class SecuritySystemSettingsExtensions
{
    extension(ISecuritySystemSettings securitySystemSettings)
    {
	    public ISecuritySystemSettings AddGeneralPermission<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
		    TSecurityContextObjectIdent>(
		    GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> info)
		    where TPrincipal : class
		    where TPermission : class
		    where TSecurityRole : class
		    where TPermissionRestriction : class
		    where TSecurityContextType : class
		    where TSecurityContextObjectIdent : notnull
	    {
		    throw new NotImplementedException();
	    }
	}
}
