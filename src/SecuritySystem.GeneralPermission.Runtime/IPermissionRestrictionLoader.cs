using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionLoader<TPermission, TPermissionRestriction>
{
    Task<List<TPermissionRestriction>> LoadAsync(TPermission permission, CancellationToken cancellationToken);

    async Task<PermissionData<TPermission, TPermissionRestriction>> ToPermissionData(TPermission dbPermission, CancellationToken cancellationToken)
    {
        var dbRestrictions = await this.LoadAsync(dbPermission, cancellationToken);

        return new PermissionData<TPermission, TPermissionRestriction>(dbPermission, dbRestrictions);
    }
}