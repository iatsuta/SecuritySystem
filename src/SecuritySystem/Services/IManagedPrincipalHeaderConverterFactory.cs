namespace SecuritySystem.Services;

public interface IManagedPrincipalHeaderConverterFactory<TPrincipal>
{
    IManagedPrincipalHeaderConverter<TPrincipal> Create(PermissionBindingInfo bindingInfo);
}