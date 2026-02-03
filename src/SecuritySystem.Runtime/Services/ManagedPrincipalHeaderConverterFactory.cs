using CommonFramework;

namespace SecuritySystem.Services;

public class ManagedPrincipalHeaderConverterFactory<TPrincipal>(IServiceProxyFactory serviceProxyFactory)
    : IManagedPrincipalHeaderConverterFactory<TPrincipal>
{
    public IManagedPrincipalHeaderConverter<TPrincipal> Create(PermissionBindingInfo bindingInfo)
    {
        return serviceProxyFactory.Create<IManagedPrincipalHeaderConverter<TPrincipal>, ManagedPrincipalHeaderConverter<TPrincipal>>(
            Tuple.Create<PermissionBindingInfo?>(bindingInfo));
    }
}