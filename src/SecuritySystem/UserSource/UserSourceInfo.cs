using System.Linq.Expressions;

namespace SecuritySystem.UserSource;

public record UserSourceInfo<TUser>(Expression<Func<TUser, string>> NamePath, Expression<Func<TUser, bool>> Filter) : UserSourceInfo
{
    public override Type UserType { get; } = typeof(TUser);
}

public abstract record UserSourceInfo
{
    public abstract Type UserType { get; }
}
