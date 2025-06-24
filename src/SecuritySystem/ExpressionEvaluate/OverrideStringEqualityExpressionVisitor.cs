using System.Linq.Expressions;
using System.Reflection;

using CommonFramework;
using CommonFramework.Maybe;

namespace SecuritySystem.ExpressionEvaluate;

public class OverrideStringEqualityExpressionVisitor(StringComparison stringComparison) : ExpressionVisitor
{
    private static readonly MethodInfo StringEqualityMethod = typeof(string).GetEqualityMethod()!;

    private static readonly MethodInfo StringEqualsMethod = new Func<string, string, StringComparison, bool>(string.Equals).Method;


    private static readonly Dictionary<MethodInfo, MethodInfo> CallMap = new Dictionary<MethodInfo, MethodInfo>
    {
        { new Func<string, bool>("".Contains).Method, new Func<string, StringComparison, bool>("".Contains).Method },
        { new Func<string, bool>("".StartsWith).Method, new Func<string, StringComparison, bool>("".StartsWith).Method },
        { new Func<string, bool>("".EndsWith).Method, new Func<string, StringComparison, bool>("".EndsWith).Method }
    };


    private readonly Expression stringComparisonExpr = Expression.Constant(stringComparison);


    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var request =
            
            from targetMethod in CallMap.GetMaybeValue(node.Method)

            let input = node.GetChildren().Select(this.Visit).ToArray()

            let newObj = targetMethod.IsStatic ? null : input.First()

            let newArgs = targetMethod.IsStatic ? input : input.Skip(1)

            select Expression.Call(newObj, targetMethod, newArgs.Concat([this.stringComparisonExpr]));


        return request.GetValueOrDefault(() => base.VisitMethodCall(node));
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.Method == StringEqualityMethod)
        {
            return Expression.Call(StringEqualsMethod, node.Left, node.Right, this.stringComparisonExpr);
        }
        else
        {
            return base.VisitBinary(node);
        }
    }
}