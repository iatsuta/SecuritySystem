using ExampleApp.Domain.Auth.General;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem;
using SecuritySystem.Services;
using SecuritySystem.Testing;
using SecuritySystem.UserSource;

namespace ExampleApp.IntegrationTests;

public class RunAsTests : TestBase
{
    [Fact]
    public async Task StartRunAsUser_AssignsRunAsPrincipalToCurrentUser()
    {
        // Arrange
        await this.AuthManager.For().CreatePrincipalAsync(this.CancellationToken);

        var runAsUserName = nameof(RunAsTests);
        var runAsUserIdentity = await this.AuthManager.For(runAsUserName).CreatePrincipalAsync(this.CancellationToken);
        var runAsUserId = (Guid)runAsUserIdentity.GetId();

        // Act
        await this.RootServiceProvider.GetRequiredService<ITestingEvaluator<IRunAsManager>>().EvaluateAsync(TestingScopeMode.Write, manager =>
            manager.StartRunAsUserAsync(runAsUserIdentity, this.CancellationToken));

        // Assert
        var currentUserName = await this.RootServiceProvider.GetRequiredService<ITestingEvaluator<ICurrentUser>>()
            .EvaluateAsync(TestingScopeMode.Read, async c => c.Name);

        var currentUserId = await this.RootServiceProvider.GetRequiredService<ITestingEvaluator<ICurrentUserSource<Principal>>>()
            .EvaluateAsync(TestingScopeMode.Read, async c => c.CurrentUser.Id);

        currentUserName.Should().Be(runAsUserName);
        currentUserId.Should().Be(runAsUserId);
    }


    [Fact]
    public async Task StartRunAsUser_WhenAlreadyRunningAsUser_DoesNotChangeRunAs()
    {
        // Arrange
        await this.AuthManager.For().CreatePrincipalAsync(this.CancellationToken);

        var runAsUserName = nameof(RunAsTests);
        var runAsUserIdentity = await this.AuthManager.For(runAsUserName).CreatePrincipalAsync(this.CancellationToken);
        var runAsUserId = (Guid)runAsUserIdentity.GetId();

        // Act
        await this.RootServiceProvider.GetRequiredService<ITestingEvaluator<IRunAsManager>>().EvaluateAsync(TestingScopeMode.Write, manager =>
            manager.StartRunAsUserAsync(runAsUserIdentity, this.CancellationToken));

        await this.RootServiceProvider.GetRequiredService<ITestingEvaluator<IRunAsManager>>().EvaluateAsync(TestingScopeMode.Write, manager =>
            manager.StartRunAsUserAsync(runAsUserIdentity, this.CancellationToken));

        // Assert
        var currentUserName = await this.RootServiceProvider.GetRequiredService<ITestingEvaluator<ICurrentUser>>()
            .EvaluateAsync(TestingScopeMode.Read, async c => c.Name);

        var currentUserId = await this.RootServiceProvider.GetRequiredService<ITestingEvaluator<ICurrentUserSource<Principal>>>()
            .EvaluateAsync(TestingScopeMode.Read, async c => c.CurrentUser.Id);

        currentUserName.Should().Be(runAsUserName);
        currentUserId.Should().Be(runAsUserId);
    }
}
