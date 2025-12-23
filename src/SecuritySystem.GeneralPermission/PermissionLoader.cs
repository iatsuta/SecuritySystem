using CommonFramework;
using CommonFramework.GenericRepository;

using GenericQueryable;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class PermissionLoader<TPrincipal, TPermission>(
    IQueryableSource queryableSource,
    IPermissionBindingInfoSource bindingInfoSource) : IPermissionLoader<TPrincipal, TPermission>
    where TPrincipal : class
    where TPermission : class
{
    private readonly PermissionBindingInfo<TPermission, TPrincipal> bindingInfo =
        (PermissionBindingInfo<TPermission, TPrincipal>)bindingInfoSource.GetForPermission(typeof(TPermission));

    public Task<List<TPermission>> LoadAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        return queryableSource.GetQueryable<TPermission>().Where(bindingInfo.Principal.Path.Select(p => p == principal)).GenericToListAsync(cancellationToken);
    }
}