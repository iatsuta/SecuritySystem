using FluentValidation;

using Framework.Authorization.Domain;

namespace SecuritySystem.TemplatePermission.Validation;

public interface IPermissionGeneralValidator : IValidator<TPermission>;
