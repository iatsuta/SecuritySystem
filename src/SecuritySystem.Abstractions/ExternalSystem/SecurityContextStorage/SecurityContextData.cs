namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public record SecurityContextData<TIdent>(TIdent Id, string Name, TIdent? ParentId) : ISecurityContextData<TIdent>;

public interface ISecurityContextData<out T>
{
    T Id { get; }

    string Name { get; }

    T? ParentId { get; }
}