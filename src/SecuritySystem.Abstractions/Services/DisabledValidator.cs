namespace SecuritySystem.Services;

public class DisabledValidator<T> : IValidator<T>
{
	public Task ValidateAsync(T value, CancellationToken cancellationToken) => Task.CompletedTask;
}