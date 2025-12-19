using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Testing;

public class TestingEvaluator<TService>(IServiceProvider rootServiceProvider, Func<IServiceProvider, TService> buildFunc)
    where TService : notnull
{
    [ActivatorUtilitiesConstructor]
    public TestingEvaluator(IServiceProvider rootServiceProvider)
        : this(rootServiceProvider, sp => sp.GetRequiredService<TService>())
    {

    }

    public async Task<TResult> EvaluateAsync<TResult>(Func<TService, Task<TResult>> evaluate)
    {
        await using var scope = rootServiceProvider.CreateAsyncScope();

        var service = buildFunc(scope.ServiceProvider);

        return await evaluate(service);
    }

    public async Task EvaluateAsync(Func<TService, Task> evaluate)
    {
        await this.EvaluateAsync(async service =>
        {
            await evaluate(service);

            return default(object);
        });
    }
}