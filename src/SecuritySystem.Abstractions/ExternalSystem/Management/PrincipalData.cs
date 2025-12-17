namespace SecuritySystem.ExternalSystem.Management;

public abstract record PrincipalData
{
	public abstract Type PrincipalType { get; }
}

public abstract record PrincipalData<TPrincipal>(TPrincipal Principal) : PrincipalData {
    public override Type PrincipalType { get; } = typeof(TPrincipal);
}

public record PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(
	TPrincipal Principal,
	IReadOnlyList<PermissionData<TPermission, TPermissionRestriction>> PermissionDataList) : PrincipalData<TPrincipal>(Principal);