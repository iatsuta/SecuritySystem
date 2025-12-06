namespace SecuritySystem.Services;

public class EmptySecurityValidator<T> : ISecurityValidator<T>
{
	public Task ValidateAsync(T value, CancellationToken cancellationToken) => Task.CompletedTask;
}