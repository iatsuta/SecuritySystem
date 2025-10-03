using CommonFramework.DependencyInjection;

using ExampleApp.Api.Controllers;
using ExampleApp.Domain;
using ExampleApp.Infrastructure.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace ExampleApp.IntegrationTests;

public class MainTests : IAsyncLifetime
{
    protected readonly IServiceProvider RootServiceProvider;

    public MainTests()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json", false, true).Build();

        this.RootServiceProvider =
            new ServiceCollection()
                .AddInfrastructure(configuration)
                .AddScoped<InitController>()
                .AddScoped<TestController>()
                .AddSingleton(TimeProvider.System)
                .ReplaceScoped<IRawUserAuthenticationService, TestRawUserAuthenticationService>()
                .AddValidator<DuplicateServiceUsageValidator>()
                .Validate()
                .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    public async Task InitializeAsync()
    {
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<InitController>().TestInitialize();
    }

    public async Task DisposeAsync()
    {

    }

    [Theory]
    [MemberData(nameof(Impersonate_LoadTestObjects_DataCorrected_Cases))]
    public async Task Impersonate_LoadTestObjects_DataCorrected(string runAs, string[] expectedBuList)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var testController = scope.ServiceProvider.GetRequiredService<TestController>();

        var runAsManager = scope.ServiceProvider.GetRequiredService<IRunAsManager>();
        await runAsManager.StartRunAsUserAsync(runAs, cancellationToken);

        // Act
        var currentUserLogin = await testController.GetCurrentUserLogin(cancellationToken);
        var testObjects = await testController.GetTestObjects(cancellationToken);

        // Assert
        runAs.Should().Be(currentUserLogin);

        var buNameList = testObjects.Select(v => v.BuName).OrderBy(v => v).ToList();
        expectedBuList.Should().BeEquivalentTo(buNameList);
    }

    public static IEnumerable<object?[]> Impersonate_LoadTestObjects_DataCorrected_Cases()
    {
        yield return ["TestRootUser", new[] { $"Test{nameof(BusinessUnit)}1-Child", $"Test{nameof(BusinessUnit)}2-Child" }];
        yield return ["TestEmployee1", new[] { $"Test{nameof(BusinessUnit)}1-Child" }];
        yield return ["TestEmployee2", new[] { $"Test{nameof(BusinessUnit)}2-Child" }];
    }

    [Theory]
    [MemberData(nameof(Impersonate_LoadBuByAncestorView_DataCorrected_Cases))]
    public async Task Impersonate_LoadBuByAncestorView_DataCorrected(string runAs, string[] expectedBuList)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var testController = scope.ServiceProvider.GetRequiredService<TestController>();

        var runAsManager = scope.ServiceProvider.GetRequiredService<IRunAsManager>();
        await runAsManager.StartRunAsUserAsync(runAs, cancellationToken);

        // Act
        var currentUserLogin = await testController.GetCurrentUserLogin(cancellationToken);
        var buList = await testController.GetBuList(cancellationToken);

        // Assert
        runAs.Should().Be(currentUserLogin);

        var buNameList = buList.Select(v => v.Name).OrderBy(v => v).ToList();
        expectedBuList.Should().BeEquivalentTo(buNameList);
    }

    public static IEnumerable<object?[]> Impersonate_LoadBuByAncestorView_DataCorrected_Cases()
    {
        yield return ["TestRootUser", new[] { $"Test{nameof(BusinessUnit)}1-Child", $"Test{nameof(BusinessUnit)}2-Child" }];
        yield return ["TestEmployee1", new[] { $"Test{nameof(BusinessUnit)}1-Child" }];
        yield return ["TestEmployee2", new[] { $"Test{nameof(BusinessUnit)}2-Child" }];
    }
}