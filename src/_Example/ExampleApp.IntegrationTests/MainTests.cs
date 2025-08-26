using CommonFramework.DependencyInjection;
using ExampleApp.Domain;
using ExampleApp.Infrastructure.DependencyInjection;
using ExampleWebApp.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace ExampleApp.IntegrationTests;

public class MainTests : IAsyncLifetime
{
    private readonly IServiceProvider rootServiceProvider;

    public MainTests()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json", false, true).Build();

        this.rootServiceProvider =
            new ServiceCollection()
                .AddInfrastructure(configuration)
                .ValidateDuplicateDeclaration()
                .AddScoped<InitController>()
                .AddScoped<TestController>()
                .AddSingleton(TimeProvider.System)
                .ReplaceScoped<IRawUserAuthenticationService, TestRawUserAuthenticationService>()
                .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    public async Task InitializeAsync()
    {
        await using var scope = this.rootServiceProvider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<InitController>().TestInitialize();
    }

    public async Task DisposeAsync()
    {

    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public async Task Impersonate_LoadData_DataCorrected(string runAs, string[] expectedBuList)
    {
        // Arrange
        await using var scope = this.rootServiceProvider.CreateAsyncScope();
        var scopeSp = scope.ServiceProvider;

        var testController = scopeSp.GetRequiredService<TestController>();

        var runAsManager = scopeSp.GetRequiredService<IRunAsManager>();
        await runAsManager.StartRunAsUserAsync(runAs);

        // Act
        var currentUserLogin = await testController.GetCurrentUserLogin();
        var testObjects = await testController.GetTestObjects();

        // Assert
        runAs.Should().Be(currentUserLogin);

        var buList = testObjects.Select(v => v.BuName).OrderBy(v => v).ToList();
        expectedBuList.Should().BeEquivalentTo(buList);
    }

    public static IEnumerable<object?[]> GetTestData()
    {
        yield return ["TestRootUser", new[] { $"Test{nameof(BusinessUnit)}1", $"Test{nameof(BusinessUnit)}2" }];
        yield return ["TestEmployee1", new[] { $"Test{nameof(BusinessUnit)}1" }];
        yield return ["TestEmployee2", new[] { $"Test{nameof(BusinessUnit)}2" }];
    }
}