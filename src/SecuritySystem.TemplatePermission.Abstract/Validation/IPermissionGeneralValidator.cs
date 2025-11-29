using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission.Validation;

public interface IPermissionGeneralValidator<in TPermission> : IValidator<TPermission>;