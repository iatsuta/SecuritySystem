using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission.Validation;

public interface IPrincipalUniquePermissionValidator<in TPrincipal> : IValidator<TPrincipal>;
