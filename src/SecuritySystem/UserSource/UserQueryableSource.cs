using CommonFramework.GenericRepository;
using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserQueryableSource<TUser>(
	IQueryableSource queryableSource,
	UserSourceInfo<TUser> userSourceInfo,
	IUserFilterFactory<TUser> userFilterFactory,
	IDefaultUserConverter<TUser> defaultUserConverter) : IUserQueryableSource<TUser>
	where TUser : class
{
	public IQueryable<TUser> GetQueryable(UserCredential userCredential)
	{
		return queryableSource
			.GetQueryable<TUser>()
			.Where(userSourceInfo.FilterPath)
			.Where(userFilterFactory.CreateFilter(userCredential));
	}

	public IUserQueryableSource<User> ToSimple()
	{
		return new SimpleUserQueryableSource(this.GetQueryable, defaultUserConverter);
	}

	public class SimpleUserQueryableSource(Func<UserCredential, IQueryable<TUser>> getFilteredQueryable, IDefaultUserConverter<TUser> defaultUserConverter)
		: IUserQueryableSource<User>
	{
		public IQueryable<User> GetQueryable(UserCredential userCredential) =>
			getFilteredQueryable(userCredential).Select(defaultUserConverter.ConvertExpression);

		public IUserQueryableSource<User> ToSimple() => this;
	}
}