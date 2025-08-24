using System.Linq.Expressions;

namespace SecuritySystem.ExternalSystem;

public interface IPermissionSource
{
    bool HasAccess();

    List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes);
}

public interface IPermissionSource<TPermission> : IPermissionSource
{
    IQueryable<TPermission> GetPermissionQuery();

    IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter);
}
