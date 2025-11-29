namespace SecuritySystem.Services;

public interface IValidator<in T>
{
	Task ValidateAsync(T value, CancellationToken cancellationToken);
}