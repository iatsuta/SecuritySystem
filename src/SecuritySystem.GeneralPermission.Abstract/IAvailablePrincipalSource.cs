namespace SecuritySystem.GeneralPermission;

public interface IAvailablePrincipalSource<out TPrincipal>
{
	IQueryable<TPrincipal> GetAvailablePrincipalsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}