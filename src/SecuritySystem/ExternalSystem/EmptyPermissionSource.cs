using System.Linq.Expressions;

namespace SecuritySystem.ExternalSystem;

public class EmptyPermissionSource<TPermission> : IPermissionSource<TPermission>
{
    public bool HasAccess() => false;

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> _) => [];

    public IQueryable<TPermission> GetPermissionQuery() => Enumerable.Empty<TPermission>().AsQueryable();

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> _) => [];
}
