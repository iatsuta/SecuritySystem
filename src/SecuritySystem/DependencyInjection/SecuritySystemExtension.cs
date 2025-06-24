using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.DependencyInjection;

public class SecuritySystemExtension(Action<IServiceCollection> addServicesAction) : ISecuritySystemExtension
{
    public void AddServices(IServiceCollection services) => addServicesAction(services);
}
