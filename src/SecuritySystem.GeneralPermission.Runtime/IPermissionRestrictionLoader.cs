using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionLoader<TPermission, TPermissionRestriction>
{
    IAsyncEnumerable<TPermissionRestriction> LoadAsync(TPermission permission);

    async Task<PermissionData<TPermission, TPermissionRestriction>> ToPermissionData(TPermission dbPermission, CancellationToken cancellationToken)
    {
        var dbRestrictions = await this.LoadAsync(dbPermission).ToArrayAsync(cancellationToken);

        return new PermissionData<TPermission, TPermissionRestriction>(dbPermission, dbRestrictions);
    }
}