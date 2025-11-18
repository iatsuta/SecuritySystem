using FluentValidation;

using Framework.Authorization.Domain;

namespace SecuritySystem.TemplatePermission.Validation;

public interface IPrincipalUniquePermissionValidator : IValidator<TPrincipal>;
