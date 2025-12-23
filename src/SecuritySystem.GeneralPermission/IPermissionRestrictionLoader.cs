namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionLoader<in TPermission, TPermissionRestriction>
{
    Task<List<TPermissionRestriction>> LoadAsync(TPermission permission, CancellationToken cancellationToken);
}