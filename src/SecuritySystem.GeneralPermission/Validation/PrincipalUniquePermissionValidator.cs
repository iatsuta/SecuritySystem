using CommonFramework;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public class PrincipalUniquePermissionValidator : ISecurityValidator<PrincipalData>
{
    public Task ValidateAsync(PrincipalData value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public class PrincipalUniquePermissionValidator<TPrincipal, TPermission, TPermissionRestriction>(
	IVisualIdentityInfoSource visualIdentityInfoSource,
	IDisplayPermissionService<TPermission, TPermissionRestriction> displayPermissionService,
	IEqualityComparer<PermissionData<TPermission, TPermissionRestriction>> comparer)
	: ISecurityValidator<PrincipalData<TPrincipal, TPermission, TPermissionRestriction>>
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

		if (duplicates.Count > 1)
		{
			var messageBody = duplicates.Join(",", g => $"({displayPermissionService.ToString(g.Key)})");

			var message = $"Principal \"{principalVisualIdentityInfo.Name.Getter(principalData.Principal)}\" has duplicate permissions: {messageBody}";

			throw new SecuritySystemException(message);
		}
	}
}