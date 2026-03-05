namespace SecuritySystem.GeneralPermission;

public interface IPermissionLoader<in TPrincipal, out TPermission>
{
    IAsyncEnumerable<TPermission> LoadAsync(TPrincipal principal);
}