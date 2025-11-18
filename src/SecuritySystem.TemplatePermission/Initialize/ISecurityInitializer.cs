namespace SecuritySystem.TemplatePermission.Initialize;

public interface ISecurityInitializer
{
    Task Init(CancellationToken cancellationToken = default);
}
