using System.Linq.Expressions;

namespace SecuritySystem.ExpressionEvaluate;

public interface ILambdaCompileCache
{
    TDelegate GetFunc<TDelegate>(Expression<TDelegate> lambdaExpression);
}