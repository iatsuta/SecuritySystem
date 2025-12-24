namespace SecuritySystem.ExternalSystem.Management;

public abstract record PrincipalData
{
	public abstract Type PrincipalType { get; }

    public abstract Type PermissionType { get; }

    public abstract Type PermissionRestrictionType { get; }
}

public record PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(
    TPrincipal Principal,
    IReadOnlyList<PermissionData<TPermission, TPermissionRestriction>> PermissionDataList) : PrincipalData<TPrincipal>(Principal)
{
    public PrincipalData(TPrincipal principal, IEnumerable<PermissionData<TPermission, TPermissionRestriction>> permissionDataList)
        : this(principal, permissionDataList.ToList())
    {
    }

    public override Type PermissionType { get; } = typeof(TPrincipal);

    public override Type PermissionRestrictionType { get; } = typeof(TPrincipal);
}

public abstract record PrincipalData<TPrincipal>(TPrincipal Principal) : PrincipalData
{
    public override Type PrincipalType { get; } = typeof(TPrincipal);
}
