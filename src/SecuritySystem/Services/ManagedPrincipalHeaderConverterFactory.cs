using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Services;

public class ManagedPrincipalHeaderConverterFactory<TPrincipal>(IServiceProvider serviceProvider) : IManagedPrincipalHeaderConverterFactory<TPrincipal>
{
    public IManagedPrincipalHeaderConverter<TPrincipal> Create(PermissionBindingInfo bindingInfo)
    {
        return ActivatorUtilities.CreateInstance<ManagedPrincipalHeaderConverter<TPrincipal>>(serviceProvider,
            Tuple.Create<PermissionBindingInfo?>(bindingInfo));
    }
}