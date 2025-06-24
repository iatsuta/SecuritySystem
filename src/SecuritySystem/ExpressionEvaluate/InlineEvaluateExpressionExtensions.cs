using CommonFramework;
using CommonFramework.Maybe;

using System.Linq.Expressions;
using System.Reflection;

namespace SecuritySystem.ExpressionEvaluate;

public static class InlineEvaluateExpressionExtensions
{
    /// <summary>
    /// Встраивает вызовы Eval-методов непосредственно в сам текущий Expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expr"></param>
    /// <returns></returns>
    public static Expression<T> InlineEvaluate<T>(this Expression<T> expr)
    {
        return expr.UpdateBody(new InlineEvaluateExpressionVisitor());
    }

    private sealed class InlineEvaluateExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo[] EvalMethods = typeof(IExpressionEvaluator).GetMethods().Where(method => method.Name == "Evaluate").ToArray();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var baseVisited = base.VisitMethodCall(node);

            return TryInlineEvaluate(baseVisited)
                       .Or(() => this.VisitExpressionArguments(baseVisited))
                       .GetValueOrDefault(baseVisited);
        }

        private static Maybe<Expression> TryInlineEvaluate(Expression baseNode)
        {
            return from node in (baseNode as MethodCallExpression).ToMaybe()

                   where EvalMethods.Any(evalMethod => node.Method.IsGenericMethodImplementation(evalMethod))

                   from evalLambda in node.Arguments.First().GetMemberConstValue<LambdaExpression>()

                   select node.Arguments.Skip(1)
                              .ZipStrong(evalLambda.Parameters, (arg, param) => new { arg, param })
                              .Aggregate(evalLambda.Body, (state, pair) => state.Override(pair.param, pair.arg));
        }

        private Maybe<Expression> VisitExpressionArguments(Expression baseNode)
        {
            return from node in (baseNode as MethodCallExpression).ToMaybe()

                   let visitedArguments = node.Arguments.Select(arg => new { arg, visitedArg = this.Visit(arg) }).ToList()

                   where visitedArguments.Any(pair => pair.arg != pair.visitedArg)

                   select (Expression)Expression.Call(node.Object, node.Method, visitedArguments.Select(pair => pair.visitedArg));
        }
    }
}