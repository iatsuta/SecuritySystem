using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

using SecuritySystem.Credential;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class AvailablePermissionSource<TPermission>() : IAvailablePermissionSource<TPermission>
{

    public IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        throw new NotImplementedException();
    }
}

public class AvailablePermissionSource<TPrincipal, TPermission, TSecurityContextObjectIdent>(
    IQueryableSource queryableSource,
    TimeProvider timeProvider,
    IUserNameResolver<TPrincipal> userNameResolver,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver,
    ISecurityIdentityConverter<TSecurityContextObjectIdent> securityIdentityConverter,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    )
    : IAvailablePermissionSource<TPermission>
    where TPermission : class
    where TSecurityContextObjectIdent : notnull
{
    public Expression<Func<TPermission, bool>> ToFilterExpression(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        throw new NotImplementedException();
    }



    public IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        throw new NotImplementedException();

        //var filter = this.CreateFilter(securityRule);

        //return this.GetAvailablePermissionsQueryable(filter);
    }




    //public IQueryable<TPermission> GetAvailablePermissionsQueryable(AvailablePermissionFilter<TSecurityContextObjectIdent> filter)
    //{
    //    return queryableSource.GetQueryable<TPermission>().Where(this.ToFilterExpression(filter));
    //}


    //private Expression<Func<TPermission, bool>> ToFilterExpression(AvailablePermissionFilter<TSecurityContextObjectIdent> filter)
    //{
    //    return this.GetFilterExpressionElements(filter).BuildAnd();
    //}


}
