using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;

namespace SecuritySystem.UserSource;

public record UserPathInfo<TUser>(
    Expression<Func<TUser, Guid>> IdPath,
    Expression<Func<TUser, string>> NamePath,
    Expression<Func<TUser, bool>> Filter) : IUserPathInfo
{
    public Type UserDomainObjectType { get; } = typeof(TUser);

    public Expression<Func<TUser, User>> ToDefaultUserExpr { get; } =

        ExpressionEvaluateHelper.InlineEvaluate(ee =>

            ExpressionHelper.Create((TUser user) => new User(ee.Evaluate(IdPath, user), ee.Evaluate(NamePath, user))));
}