using CommonFramework;
using CommonFramework.ExpressionComparers;

using System.Linq.Expressions;

namespace SecuritySystem.ExpressionEvaluate;

public class LambdaCompileCache(LambdaCompileMode mode) : ILambdaCompileCache
{
    private readonly Dictionary<LambdaExpression, Delegate> cache = new Dictionary<LambdaExpression, Delegate>(LambdaComparer.Value);

    private readonly Lock locker = new Lock();


    public TDelegate GetFunc<TDelegate>(Expression<TDelegate> lambdaExpression)
    {
        IReadOnlyCollection<Tuple<ParameterExpression, ConstantExpression>> args = null!;

        var newLambdaExpression =

            lambdaExpression.Pipe(mode.HasFlag(LambdaCompileMode.OptimizeBooleanLogic), lambda => lambda.Optimize())
                .Pipe(lambda => ConstantToParameters(lambda, out args));

        var getDelegateFunc = this.GetGetDelegate(newLambdaExpression, args!.Select(v => v.Item1));

        return (TDelegate)getDelegateFunc.DynamicInvoke(args.Select(v => v.Item2.Value).ToArray());
    }

    private Delegate GetGetDelegate(LambdaExpression expr, IEnumerable<ParameterExpression> parameters)
    {
        lock (this.locker)
        {
            return (this.cache.GetValueOrCreate(expr, () =>
                expr.Pipe(mode.HasFlag(LambdaCompileMode.IgnoreStringCase), lambda => lambda.UpdateBodyBase(new OverrideStringEqualityExpressionVisitor(StringComparison.CurrentCultureIgnoreCase)))
                    .Pipe(mode.HasFlag(LambdaCompileMode.InjectMaybe), InjectMaybeVisitor.Value.VisitAndGetValueOrDefaultBase)
                    .Pipe(body => Expression.Lambda(body, parameters))
                    .Compile()));
        }
    }


    private static LambdaExpression ConstantToParameters(LambdaExpression lambdaExpression, out IReadOnlyCollection<Tuple<ParameterExpression, ConstantExpression>> args)
    {
        var listArgs = new List<Tuple<ParameterExpression, ConstantExpression>>();

        var newExpression = lambdaExpression.UpdateBase(new ConstantToParameterExpressionVisitor(listArgs));

        args = listArgs;

        return (LambdaExpression)newExpression;
    }

    private class ConstantToParameterExpressionVisitor(List<Tuple<ParameterExpression, ConstantExpression>> args) : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var newParameter = Expression.Parameter(node.Type, "OverrideConst_" + args.Count);

            args.Add(Tuple.Create(newParameter, node));

            return newParameter;
        }
    }
}