using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.Permission;

public interface IPermissionValidator<TPermission, TPermissionRestriction> : ISecurityValidator<PermissionData<TPermission, TPermissionRestriction>>;