namespace SecuritySystem.Testing;

public static class TestingEvaluatorExtensions
{
    extension<TService>(ITestingEvaluator<TService> testingEvaluator)
    {
        public async Task EvaluateAsync(Func<TService, Task> evaluate)
        {
            await testingEvaluator.EvaluateAsync(async service =>
            {
                await evaluate(service);

                return default(object);
            });
        }

        public ITestingEvaluator<TNewService> Select<TNewService>(Func<TService, TNewService> selector)
        {
            return testingEvaluator.Select(async service => selector(service));
        }

        public ITestingEvaluator<TNewService> Select<TNewService>(Func<TService, Task<TNewService>> selector)
        {
            return new ChangedTestingEvaluator<TService, TNewService>(testingEvaluator, selector);
        }
    }

    private class ChangedTestingEvaluator<TService, TNewService>(ITestingEvaluator<TService> baseEvaluator, Func<TService, Task<TNewService>> selector)
        : ITestingEvaluator<TNewService>
    {
        public async Task<TResult> EvaluateAsync<TResult>(Func<TNewService, Task<TResult>> evaluate)
        {
            return await baseEvaluator.EvaluateAsync(async baseService =>
            {
                var newService = await selector(baseService);

                return await evaluate(newService);
            });
        }
    }
}