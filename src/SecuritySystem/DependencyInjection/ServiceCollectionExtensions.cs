using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSecuritySystem(Action<ISecuritySystemSettings> setupAction)
        {
            var settings = new SecuritySystemSettings();

            setupAction(settings);

            settings.Initialize(services);

            return services;
        }
    }
}