using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IAvailablePermissionSource<out TPermission>
{
	IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}

public interface IAvailablePermissionSource<TPermission, TSecurityContextObjectIdent> : IAvailablePermissionSource<TPermission>
{
	Expression<Func<TPermission, bool>> ToFilterExpression(AvailablePermissionFilter<TSecurityContextObjectIdent> filter);

	AvailablePermissionFilter<TSecurityContextObjectIdent> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule);

	IQueryable<TPermission> GetAvailablePermissionsQueryable(AvailablePermissionFilter<TSecurityContextObjectIdent> filter);
}