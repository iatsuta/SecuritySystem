using System.Linq.Expressions;

namespace SecuritySystem.ExpressionEvaluate;

public class FakeExpressionEvaluator : IExpressionEvaluator
{
    public TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
    {
        throw new NotImplementedException();
    }
}