using CommonFramework;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.Principal;

public class PrincipalUniquePermissionValidator<TPrincipal, TPermission, TPermissionRestriction>(
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IDisplayPermissionService<TPermission, TPermissionRestriction> displayPermissionService,
    IPermissionEqualityComparer<TPermission, TPermissionRestriction> comparer)
    : IPrincipalValidator<TPrincipal, TPermission, TPermissionRestriction>
{
    private readonly VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>();

    public async Task ValidateAsync(PrincipalData<TPrincipal, TPermission, TPermissionRestriction> principalData, CancellationToken cancellationToken)
    {
        var duplicates = await principalData
            .PermissionDataList
            .GroupBy(permission => permission, comparer)
            .Where(g => g.Count() > 1)
            .ToAsyncEnumerable()
            .ToListAsync(cancellationToken);

        if (duplicates.Count > 0)
        {
            var messageBody = duplicates.Join(",", g => $"({displayPermissionService.ToString(g.Key)})");

            var message = $"Principal \"{principalVisualIdentityInfo.Name.Getter(principalData.Principal)}\" has duplicate permissions: {messageBody}";

            throw new SecuritySystemValidationException(message);
        }
    }
}