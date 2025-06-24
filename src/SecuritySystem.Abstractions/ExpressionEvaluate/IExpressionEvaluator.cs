using System.Linq.Expressions;

namespace SecuritySystem.ExpressionEvaluate;

public interface IExpressionEvaluator
{
    TResult Evaluate<TSource, TResult>(Expression<Func<TSource, TResult>> expression, TSource arg) => this.Compile(expression).Invoke(arg);

    TResult Evaluate<TArg1, TArg2, TResult>(Expression<Func<TArg1, TArg2, TResult>> expression, TArg1 arg1, TArg2 arg2) => this.Compile(expression).Invoke(arg1, arg2);

    TDelegate Compile<TDelegate>(Expression<TDelegate> expression);
}