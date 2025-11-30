using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission.Validation;

public interface IPermissionGeneralValidator<in TPermission> : IValidator<TPermission>;