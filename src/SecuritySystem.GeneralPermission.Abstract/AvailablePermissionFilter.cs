using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public record AvailablePermissionFilter<TSecurityContextObjectIdent>
{
	public required DateTime Date { get; init; }

	public string? PrincipalName { get; init; }

	public required IReadOnlyList<TSecurityContextObjectIdent>? SecurityRoleIdents { get; init; }

	public required IReadOnlyDictionary<
		TSecurityContextObjectIdent,
		(bool AllowGrandAccess, Expression<Func<TSecurityContextObjectIdent, bool>>
		RestrictionFilterExpr)> RestrictionFilters { get; init; }
}