namespace ExampleApp.Domain;

public class Location : SecuritySystem.ISecurityContext
{
    public int MyId { get; set; }

    public required string Name { get; set; }
}