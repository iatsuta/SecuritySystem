using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Testing.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSecuritySystemTesting(Action<ISecuritySystemTestingBuilder>? setup = null)
        {
            var builder = new SecuritySystemTestingBuilder();

            setup?.Invoke(builder);

            builder.Initialize(services);

            return services;
        }
    }
}