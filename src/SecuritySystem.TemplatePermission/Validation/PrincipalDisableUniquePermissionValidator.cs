using FluentValidation;

using Framework.Authorization.Domain;

namespace SecuritySystem.TemplatePermission.Validation;

public class PrincipalDisableUniquePermissionValidator : AbstractValidator<Principal>, IPrincipalUniquePermissionValidator;
