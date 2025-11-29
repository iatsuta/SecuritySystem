namespace SecuritySystem.TemplatePermission.Initialize;

public class SecurityIdentityConverter<TIdent> : ISecurityIdentityConverter<TIdent>
	where TIdent : notnull
{
	public SecurityIdentity<TIdent> Convert(SecurityIdentity securityIdentity)
	{
		if (securityIdentity is SecurityIdentity<TIdent> typedSecurityIdentity)
		{
			return typedSecurityIdentity;
		}
		else
		{
			throw new NotImplementedException();
		}
	}
}