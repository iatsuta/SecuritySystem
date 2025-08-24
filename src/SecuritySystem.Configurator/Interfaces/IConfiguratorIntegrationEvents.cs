namespace SecuritySystem.Configurator.Interfaces;

public interface IConfiguratorIntegrationEvents
{
    Task PrincipalCreatedAsync(object principal, CancellationToken cancellationToken = default);

    Task PrincipalChangedAsync(object principal, CancellationToken cancellationToken = default);

    Task PrincipalRemovedAsync(object principal, CancellationToken cancellationToken = default);

    Task PermissionCreatedAsync(object permission, CancellationToken cancellationToken = default);

    Task PermissionChangedAsync(object permission, CancellationToken cancellationToken = default);

    Task PermissionRemovedAsync(object permission, CancellationToken cancellationToken = default);
}
