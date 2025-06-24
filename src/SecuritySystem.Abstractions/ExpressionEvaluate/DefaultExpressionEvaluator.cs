using System.Linq.Expressions;

namespace SecuritySystem.ExpressionEvaluate;

public class DefaultExpressionEvaluator : IExpressionEvaluator
{
    public TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
    {
        return expression.Compile();
    }
}