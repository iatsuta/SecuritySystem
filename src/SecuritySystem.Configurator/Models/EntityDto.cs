namespace SecuritySystem.Configurator.Models;

public record EntityDto
{
    public required string Id { get; init; }

	public required string Name { get; init; }
}