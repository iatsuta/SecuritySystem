using CommonFramework.DependencyInjection;

using ExampleApp.Api.Controllers;
using ExampleApp.Infrastructure.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Testing;
using SecuritySystem.Testing.DependencyInjection;

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

                .AddSecuritySystemTesting()

                .AddValidator<DuplicateServiceUsageValidator>()
                .Validate()
                .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected RootAuthManager AuthManager => this.RootServiceProvider.GetRequiredService<RootAuthManager>();

    protected ITestingUserAuthenticationService AuthenticationService => this.RootServiceProvider.GetRequiredService<ITestingUserAuthenticationService>();

    public virtual async ValueTask InitializeAsync()
    {
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<InitController>().TestInitialize(CancellationToken);
    }

    public virtual async ValueTask DisposeAsync()
    {
    }
}