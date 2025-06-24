using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.DependencyInjection;

public interface ISecuritySystemExtension
{
    public void AddServices(IServiceCollection services);
}
