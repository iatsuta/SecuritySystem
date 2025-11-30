using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public interface IPrincipalGeneralValidator<in TPrincipal> : IValidator<TPrincipal>;
