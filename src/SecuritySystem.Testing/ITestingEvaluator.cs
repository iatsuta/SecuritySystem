namespace SecuritySystem.Testing;

public interface ITestingEvaluator<out TService>
{
    Task<TResult> EvaluateAsync<TResult>(TestingScopeMode mode, Func<TService, Task<TResult>> evaluate);
}