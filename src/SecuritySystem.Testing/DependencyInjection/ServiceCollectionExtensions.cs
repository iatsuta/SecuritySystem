using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace SecuritySystem.Testing.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSecuritySystemTesting()
        {
            return services.AddScoped<IUserCredentialNameResolver, UserCredentialNameResolver>()
                .AddScoped<TestRawUserAuthenticationService>()
                .ReplaceScopedFrom<IRawUserAuthenticationService, TestRawUserAuthenticationService>()
                .AddSingleton<RootAuthManager>()
                .AddValidator<DuplicateServiceUsageValidator>();
        }
    }
}