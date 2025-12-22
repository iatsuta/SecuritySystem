namespace ExampleApp.Domain.Auth.General;

public class Principal
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;
}