using CommonFramework;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public interface IPermissionSecurityRoleResolver<in TPermission>
{
	FullSecurityRole ResolveRole(TPermission permission);
}

public class PermissionRequiredContextValidator<TPermission, TPermissionRestriction>(
	IPermissionSecurityRoleResolver<TPermission> permissionSecurityRoleResolver)
	: ISecurityValidator<PermissionData<TPermission, TPermissionRestriction>>
{
	public Task ValidateAsync(PermissionData<TPermission, TPermissionRestriction> permissionData, CancellationToken cancellationToken)
	{
	//	TypedPrincipal

		var securityRole = permissionSecurityRoleResolver.ResolveRole(permissionData.Permission);

		if (securityRole.Information.Restriction.SecurityContextRestrictions is {} restrictions)
		{
			var requiredSecurityContextTypes = restrictions.Where(pair => pair.Required).Select(pair => pair.SecurityContextType);

			var usedTypes = permissionData.Restrictions.Select(r => r.SecurityContextType).Distinct()
				.Select(sct => securityContextInfoSource.GetSecurityContextInfo(sct.Id).Type);

			var missedTypeInfoList = requiredSecurityContextTypes
				.Except(usedTypes)
				.Select(securityContextInfoSource.GetSecurityContextInfo)
				.Select(info => info.Name)
				.ToList();

			context.MessageFormatter.AppendArgument("MissedTypes", missedTypeInfoList.Join(", "));

			return missedTypeInfoList.Count == 0;
		}
		else
		{
			return true;
		}

		this.RuleFor(permission => permission.Restrictions)
			.Must((permission, _, context) =>
			{
				var role = securityRoleSource.GetSecurityRole(permission.Role.Id);


			})
			.WithMessage($"{nameof(TPermission)} must contain the required contexts: {{MissedTypes}}");
	}
}