namespace ExampleApp.Domain.Auth.General;

public class SecurityRole
{
    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public required string Description { get; set; }
}