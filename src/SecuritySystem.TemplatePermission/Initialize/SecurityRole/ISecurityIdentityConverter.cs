namespace SecuritySystem.TemplatePermission.Initialize;

public interface ISecurityIdentityConverter<TIdent>
	where TIdent : notnull
{
	SecurityIdentity<TIdent> Convert(SecurityIdentity securityIdentity);
}