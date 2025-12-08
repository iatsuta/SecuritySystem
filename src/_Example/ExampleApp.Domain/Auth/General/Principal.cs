namespace ExampleApp.Domain.Auth.General;

public class Principal
{
	public required Guid Id { get; init; }

	public required string Name { get; set; }
}