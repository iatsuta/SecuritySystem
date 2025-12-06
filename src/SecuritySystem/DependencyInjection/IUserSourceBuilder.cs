using System.Linq.Expressions;

using SecuritySystem.UserSource;

namespace SecuritySystem.DependencyInjection;

public interface IUserSourceBuilder<TUser>
{
	IUserSourceBuilder<TUser> SetName(Expression<Func<TUser, string>> namePath);

	IUserSourceBuilder<TUser> SetFilter(Expression<Func<TUser, bool>> filterPath);

	IUserSourceBuilder<TUser> SetRunAs(Expression<Func<TUser, TUser?>> runAsPath);

	IUserSourceBuilder<TUser> SetMissedService<TService>()
		where TService : IMissedUserService<TUser>;
}