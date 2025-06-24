namespace SecuritySystem.Services;

public interface ICurrentUser
{
    Guid Id { get; }

    string Name { get; }
}