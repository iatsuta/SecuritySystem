namespace SecuritySystem.GeneralPermission;

public interface IPermissionLoader<in TPrincipal, TPermission>
{
    Task<List<TPermission>> LoadAsync(TPrincipal principal, CancellationToken cancellationToken);
}