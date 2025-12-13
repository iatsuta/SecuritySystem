using ExampleApp.Api.Controllers;
using ExampleApp.Domain;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace ExampleApp.IntegrationTests;

public class VirtualPermissionTests : TestBase
{
    [Theory]
    [MemberData(nameof(Impersonate_LoadTestObjects_DataCorrected_Cases))]
    public async Task Impersonate_LoadTestObjects_DataCorrected(string runAs, string[] expectedBuList)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
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
        expectedBuList.OrderBy(v => v).Should().BeEquivalentTo(buNameList);
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
        var cancellationToken = TestContext.Current.CancellationToken;
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
        expectedBuList.OrderBy(v => v).Should().BeEquivalentTo(buNameList);
    }

    public static IEnumerable<object?[]> Impersonate_LoadBuByAncestorView_DataCorrected_Cases()
    {
        var rootBu = "TestRootBu";

        var bu_1 = $"Test{nameof(BusinessUnit)}1";
        var bu_1_1 = $"Test{nameof(BusinessUnit)}1-Child";

        var bu_2 = $"Test{nameof(BusinessUnit)}2";
        var bu_2_1 = $"Test{nameof(BusinessUnit)}2-Child";


        yield return ["TestRootUser", new[] { rootBu, bu_1, bu_1_1, bu_2, bu_2_1 }];
        yield return ["TestEmployee1", new[] { rootBu, bu_1, bu_1_1 }];
        yield return ["TestEmployee2", new[] { rootBu, bu_2, bu_2_1 }];
    }
}