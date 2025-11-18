using FluentValidation;

using Framework.Authorization.Domain;

namespace SecuritySystem.TemplatePermission.Validation;

public interface IPrincipalGeneralValidator : IValidator<TPrincipal>;
