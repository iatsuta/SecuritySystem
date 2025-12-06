namespace SecuritySystem.GeneralPermission.Validation;

public record PermissionData<TPermission, TPermissionRestriction>(TPermission Permission, IReadOnlyList<TPermissionRestriction> Restrictions);