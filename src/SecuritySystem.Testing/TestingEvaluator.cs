using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Testing;

public class TestingEvaluator<TService>(IServiceProvider rootServiceProvider) : ITestingEvaluator<TService>
    where TService : notnull
{
    public async Task<TResult> EvaluateAsync<TResult>(Func<TService, Task<TResult>> evaluate)
    {
        await using var scope = rootServiceProvider.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService<TService>();

        return await evaluate(service);
    }
}