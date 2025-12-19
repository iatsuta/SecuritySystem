using CommonFramework.DependencyInjection;

using ExampleApp.Api.Controllers;
using ExampleApp.Infrastructure.DependencyInjection;
using ExampleApp.IntegrationTests.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;
using SecuritySystem.Testing;

namespace ExampleApp.IntegrationTests;

public abstract class TestBase : IAsyncLifetime
{
    protected readonly IServiceProvider RootServiceProvider;

    protected TestBase()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json", false, true).Build();

        this.RootServiceProvider =
            new ServiceCollection()
                .AddInfrastructure(configuration)
                .AddScoped<InitController>()
                .AddScoped<TestController>()
                .AddSingleton(TimeProvider.System)

                .AddScoped<IUserCredentialNameResolver, UserCredentialNameResolver>()
                .AddScoped<TestRawUserAuthenticationService>()
                .ReplaceScopedFrom<IRawUserAuthenticationService, TestRawUserAuthenticationService>()
                .AddSingleton<RootAuthManager>()
                .AddValidator<DuplicateServiceUsageValidator>()

                .Validate()
                .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    protected RootAuthManager AuthManager => this.RootServiceProvider.GetRequiredService<RootAuthManager>();

    public async ValueTask InitializeAsync()
    {
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<InitController>().TestInitialize();
    }

    public async ValueTask DisposeAsync()
    {
    }
}