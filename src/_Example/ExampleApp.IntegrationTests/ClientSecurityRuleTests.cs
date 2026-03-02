using ExampleApp.Domain;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem;
using SecuritySystem.AvailableSecurity;

namespace ExampleApp.IntegrationTests;

public class ClientSecurityRuleTests : TestBase
{
    [Fact]
    public async Task GetAvailableSecurityRules_ReturnsExpectedClientSecurityRules()
    {
        // Arrange
        var expectedResult = new DomainSecurityRule.ClientSecurityRule[]
        {
            new(nameof(BusinessUnit) + SecurityRule.View),
            new(nameof(TestObject) + SecurityRule.View)
        };

        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var availableClientSecurityRuleSource = scope.ServiceProvider.GetRequiredService<IAvailableClientSecurityRuleSource>();

        // Act
        var result = await availableClientSecurityRuleSource.GetAvailableSecurityRules(this.CancellationToken);

        // Assert
        result.OrderBy(v => v.Name).Should().BeEquivalentTo(expectedResult);
        return;
    }
}