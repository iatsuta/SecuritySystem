using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Validation;

namespace SecuritySystem.GeneralPermission.Validation.Principal;

public interface IPrincipalValidator<TPrincipal, TPermission, TPermissionRestriction> : ISecurityValidator<PrincipalData<TPrincipal, TPermission, TPermissionRestriction>>;