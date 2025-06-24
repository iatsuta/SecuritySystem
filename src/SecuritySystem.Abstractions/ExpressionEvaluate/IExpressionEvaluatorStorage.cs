namespace SecuritySystem.ExpressionEvaluate;

public interface IExpressionEvaluatorStorage
{
    IExpressionEvaluator GetForType(Type type);
}