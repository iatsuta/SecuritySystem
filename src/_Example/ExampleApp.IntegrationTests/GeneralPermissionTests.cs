using ExampleApp.Domain.Auth.General;

using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.UserSource;

namespace ExampleApp.IntegrationTests;

public class GeneralPermissionTests : TestBase
{
    [Fact]
    public async Task CratInvokeExpandWithParents_ForRootBu_DataCorrected()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        //p

        var currentUserSource = scope.ServiceProvider.GetRequiredService<ICurrentUserSource<Principal>>();
        var currentUser = currentUserSource.CurrentUser;
        // Act

        // Assert
    }


    //private async Task CreateTestPermission(IServiceProvider serviceProvider, string principalName, CancellationToken cancellationToken)
    //{
    //    var principalDomainService = serviceProvider.GetRequiredService<IPrincipalDomainService<Principal>>();

    //    var principal = await principalDomainService.GetOrCreateAsync(principalName, cancellationToken);

    //    var permission = new Permission { Principal = principal };

    //    return;

    //}
}