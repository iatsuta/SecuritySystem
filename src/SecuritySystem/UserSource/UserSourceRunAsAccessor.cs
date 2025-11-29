using System.Linq.Expressions;

namespace SecuritySystem.UserSource;

public record UserSourceRunAsInfo<TUser>(Expression<Func<TUser, TUser?>> Path)
{
	public PropertyAccessors<TUser, TUser?> Accessors { get; } = new (Path);
}