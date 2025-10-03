namespace ExampleApp.Domain;

public class BusinessUnit : SecuritySystem.ISecurityContext
{
    public Guid Id { get; set; }

    public virtual BusinessUnit? Parent { get; set; }

    public required string Name { get; set; }
}