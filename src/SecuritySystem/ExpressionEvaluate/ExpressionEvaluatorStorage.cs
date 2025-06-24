using CommonFramework.DictionaryCache;

namespace SecuritySystem.ExpressionEvaluate;

public class ExpressionEvaluatorStorage(LambdaCompileMode lambdaCompileMode) : IExpressionEvaluatorStorage
{
    private readonly IDictionaryCache<Type, IExpressionEvaluator> cache = new DictionaryCache<Type, IExpressionEvaluator>(_ =>
        new ExpressionEvaluator(new LambdaCompileCache(lambdaCompileMode)));

    public IExpressionEvaluator GetForType(Type type)
    {
        return this.cache[type];
    }
}