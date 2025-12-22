using System.Linq.Expressions;

namespace SecuritySystem.UserSource;

public record UserSourceInfo<TUser>(Expression<Func<TUser, bool>> FilterPath) : UserSourceInfo
{
	public override Type UserType { get; } = typeof(TUser);
}

public abstract record UserSourceInfo
{
	public abstract Type UserType { get; }
}