namespace ExampleApp.Domain.Auth.General;

public class SecurityContextType
{
	public required Guid Id { get; init; }

	public required string Name { get; set; }
}