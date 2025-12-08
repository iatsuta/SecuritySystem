using CommonFramework;

using SecuritySystem.ExternalSystem.SecurityContextStorage;

namespace SecuritySystem.GeneralPermission.Validation;

public class DisplayPermissionService<TPermission, TPermissionRestriction> : IDisplayPermissionService<TPermission, TPermissionRestriction>
{
	public string ToString(PermissionData<TPermission, TPermissionRestriction> permissionData)
	{
		return this.GetPermissionVisualParts(permissionData).Join(" | ");
	}

	private IEnumerable<string> GetPermissionVisualParts(PermissionData<TPermission, TPermissionRestriction> permissionData)
	{
		throw new NotImplementedException();
		//yield return $"Role: {permission.Role}";

		//yield return $"Period: {permission.Period}";

		//foreach (var securityContextTypeGroup in permission.Restrictions.GroupBy(fi => fi.SecurityContextType, fi => fi.SecurityContextId))
		//{
		//	var securityContextInfo = securityContextInfoSource.GetSecurityContextInfo(securityContextTypeGroup.Key.Id);

		//	var securityEntities = securityEntitySource
		//		.GetTyped(securityContextInfo.Type)
		//		.Pipe(v => (ITypedSecurityContextStorage<TSecurityContextObjectIdent>)v)
		//		.GetSecurityContextsByIdents(securityContextTypeGroup);

		//	yield return $"{securityContextTypeGroup.Key.Name.ToPluralize()}: {securityEntities.Select(v => v.Name).Join(", ")}";
		//}
	}
}