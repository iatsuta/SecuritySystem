using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.UserSource;

public record UserSourceInfo<TUser>(Expression<Func<TUser, string>> NamePath, Expression<Func<TUser, bool>> Filter) : UserSourceInfo
{
	public Action<TUser, string> NameSetter { get; } = NamePath.ToSetLambdaExpression().Compile();

	public override Type UserType { get; } = typeof(TUser);
}

public abstract record UserSourceInfo
{
    public abstract Type UserType { get; }
}
