namespace SecuritySystem.GeneralPermission.Validation;

public abstract record PrincipalData
{
	public abstract Type PrincipalType { get; }
}

public record PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(
	TPrincipal Principal,
	IReadOnlyList<PermissionData<TPermission, TPermissionRestriction>> PermissionDataList) : PrincipalData
{
	public override Type PrincipalType { get; } = typeof(TPrincipal);
}