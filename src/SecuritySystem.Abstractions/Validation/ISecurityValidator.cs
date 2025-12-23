namespace SecuritySystem.Validation;

public interface ISecurityValidator<in T>
{
	Task ValidateAsync(T value, CancellationToken cancellationToken);
}