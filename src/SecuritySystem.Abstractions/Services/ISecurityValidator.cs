namespace SecuritySystem.Services;

public interface ISecurityValidator<in T>
{
	Task ValidateAsync(T value, CancellationToken cancellationToken);
}