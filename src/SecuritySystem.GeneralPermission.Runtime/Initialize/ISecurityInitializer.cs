namespace SecuritySystem.GeneralPermission.Initialize;

public interface ISecurityInitializer
{
    Task Init(CancellationToken cancellationToken);
}
