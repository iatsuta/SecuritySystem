namespace ExampleApp.Domain.Auth.General;

public class PermissionRestriction
{
	public required Guid Id { get; init; }

	public required Guid SecurityContextId { get; init; }

	public virtual required SecurityContextType SecurityContextType { get; init; }

	public virtual required Permission Permission { get; init; }
}