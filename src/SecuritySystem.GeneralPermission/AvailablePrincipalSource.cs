namespace SecuritySystem.GeneralPermission;

public class AvailablePrincipalSource<TPrincipal> : IAvailablePrincipalSource<TPrincipal>
{
    public IQueryable<TPrincipal> GetAvailablePrincipalsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        throw new NotImplementedException();
    }
}