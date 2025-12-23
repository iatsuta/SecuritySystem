using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission.Validation;

public interface IPermissionEqualityComparer<TPermission, TPermissionRestriction> : IEqualityComparer<PermissionData<TPermission, TPermissionRestriction>>;