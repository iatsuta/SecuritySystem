using System.Linq.Expressions;

namespace SecuritySystem.UserSource;

public record UserSourceInfo<TUser>(PropertyAccessors<TUser, string> Name, Expression<Func<TUser, bool>> Filter) : UserSourceInfo
{
	public UserSourceInfo(Expression<Func<TUser, string>> namePath, Expression<Func<TUser, bool>> filter)
		: this(new PropertyAccessors<TUser, string>(namePath), filter)
	{
	}

	public override Type UserType { get; } = typeof(TUser);
}

public abstract record UserSourceInfo
{
	public abstract Type UserType { get; }
}