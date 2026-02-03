using System.Linq.Expressions;
using System.Reflection;

using CommonFramework;
using CommonFramework.DictionaryCache;
using CommonFramework.Maybe;

namespace SecuritySystem.Builders;

internal class CacheContainsCallVisitor : ExpressionVisitor
{
    private CacheContainsCallVisitor()
    {
    }

    public override Expression? Visit(Expression? node)
    {
        return node == null ? base.Visit(node) : node.UpdateBase(new InternalStateVisitor());
    }

    private class InternalStateVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo EnumerableContainsMethod =
            new Func<IEnumerable<Ignore>, Ignore, bool>(Enumerable.Contains).Method.GetGenericMethodDefinition();

        private static readonly IDictionaryCache<Type, MethodInfo> HashSetContainsMethods = new DictionaryCache<Type, MethodInfo>(type =>
        {
            return typeof(HashSet<>).MakeGenericType(type).GetMethods().Single(m => m.Name == nameof(HashSet<>.Contains) && m.GetParameters().Length == 1);
        }).WithLock();


        private readonly Dictionary<object, ConstantExpression> constCache = new();


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var request =

                from _ in Maybe.Return()

                let methodInfo = node.Method

                where methodInfo.IsGenericMethod && methodInfo.GetGenericMethodDefinition() == EnumerableContainsMethod

                let identType = methodInfo.GetGenericArguments().Single()

                from hashSet in GetHashSet(node.Arguments[0], identType)

                select Expression.Call(hashSet, HashSetContainsMethods[identType], node.Arguments[1]);


            return request.GetValueOrDefault(() => base.VisitMethodCall(node));
        }

        private Maybe<ConstantExpression> GetHashSet(Expression node, Type argType)
        {
            return new Func<Expression, Maybe<ConstantExpression>>(this.GetHashSet<Ignore>).CreateGenericMethod(argType)
                .Invoke<Maybe<ConstantExpression>>(this, node);
        }

        private Maybe<ConstantExpression> GetHashSet<TIdent>(Expression node)
        {
            return from enumerable in
                    node.GetDeepMemberConstValue<HashSet<TIdent>>().Select(IEnumerable<TIdent> (v) => v!)
                        .Or(() => node.GetDeepMemberConstValue<IQueryable<TIdent>>().Select(IEnumerable<TIdent> (v) => v!))

                select this.constCache.GetValueOrCreate(enumerable, () => Expression.Constant(enumerable.ToHashSet(), typeof(HashSet<TIdent>)));
        }
    }


    public static readonly CacheContainsCallVisitor Value = new CacheContainsCallVisitor();
}