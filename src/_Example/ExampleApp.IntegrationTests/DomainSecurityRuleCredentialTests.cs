using ExampleApp.Application;
using ExampleApp.Domain;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem;

namespace ExampleApp.IntegrationTests;

public class DomainSecurityRuleCredentialTests : TestBase
{
    [Theory]
    [MemberData(nameof(GetEmployees_ReturnsExpectedUsers_Cases))]
    public async Task GetEmployees_ReturnsExpectedUsers(SecurityRule securityRule, string?[] expectedUsers)
    {
        // Arrange
        var realExpectedUsers = expectedUsers.Select(userName => userName ?? this.AuthenticationService.GetUserName());

        // Act
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var employees = await scope.ServiceProvider.GetRequiredService<IRepositoryFactory<Employee>>().Create(securityRule).GetQueryable()
            .GenericToListAsync(this.CancellationToken);

        // Assert
        realExpectedUsers.OrderBy(v => v).Should().BeEquivalentTo(employees.Select(e => e.Login));
    }


    public static IEnumerable<object?[]> GetEmployees_ReturnsExpectedUsers_Cases()
    {
        string? user0 = null;
        string? user1 = "TestEmployee1";
        string? user2 = "TestEmployee2";


        yield return
        [
            DomainSecurityRule.CurrentUser,
            new[] { user0 }
        ];

        yield return
        [
            DomainSecurityRule.CurrentUser with { CustomCredential = new SecurityRuleCredential.CustomUserSecurityRuleCredential(user1) },
            new[] { user1 }
        ];

        yield return
        [
            (DomainSecurityRule.CurrentUser with { CustomCredential = new SecurityRuleCredential.CustomUserSecurityRuleCredential(user1) })
            .Or(DomainSecurityRule.CurrentUser with { CustomCredential = new SecurityRuleCredential.CustomUserSecurityRuleCredential(user2) }),
            new[] { user1, user2 }
        ];

        yield return
        [
            (DomainSecurityRule.CurrentUser with { CustomCredential = new SecurityRuleCredential.CustomUserSecurityRuleCredential(user0) })
                .Or(DomainSecurityRule.CurrentUser with { CustomCredential = new SecurityRuleCredential.CustomUserSecurityRuleCredential(user1) })
                with
                {
                    CustomCredential = new SecurityRuleCredential.CustomUserSecurityRuleCredential(user2)
                },
            new[] { user2 }
        ];
    }
}