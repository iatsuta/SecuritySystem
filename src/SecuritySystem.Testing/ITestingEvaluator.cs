namespace SecuritySystem.Testing;

public interface ITestingEvaluator<out TService>
{
    Task<TResult> EvaluateAsync<TResult>(Func<TService, Task<TResult>> evaluate);
}