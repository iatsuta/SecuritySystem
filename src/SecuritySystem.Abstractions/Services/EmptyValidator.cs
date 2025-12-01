namespace SecuritySystem.Services;

public class EmptyValidator<T> : IValidator<T>
{
	public Task ValidateAsync(T value, CancellationToken cancellationToken) => Task.CompletedTask;
}