namespace SecuritySystem.GeneralPermission;

public interface IRawPermissionRestrictionLoader<in TPermission>
{
    ValueTask<Dictionary<Type, Array>> LoadAsync(TPermission permission, CancellationToken cancellationToken);
}