using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public interface IPrincipalUniquePermissionValidator<in TPrincipal> : IValidator<TPrincipal>;
