namespace SecuritySystem.GeneralPermission;

public interface IRawPermissionRestrictionLoader<in TPermission>
{
    Task<Dictionary<Type, Array>> LoadAsync(TPermission permission, CancellationToken cancellationToken);
}