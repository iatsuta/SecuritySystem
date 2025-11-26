using CommonFramework;

namespace SecuritySystem.UserSource;

public class CurrentUserSource<TUser>(IUserSource<TUser> userSource) : ICurrentUserSource<TUser>
{
    private readonly Lazy<TUser> lazyCurrentUser = LazyHelper.Create(() => userSource.GetUser(currentUser.CurrentUser.Name));

    public TUser CurrentUser => this.lazyCurrentUser.Value;
}

public class CurrentUserSourceInternal<TUser>(ICurrentUserSource<User> currentUser, IUserSource<TUser> userSource) : ICurrentUserSource<TUser>
{
	private readonly Lazy<TUser> lazyCurrentUser = LazyHelper.Create(() => userSource.GetUser(currentUser.CurrentUser.Name));

	public TUser CurrentUser => this.lazyCurrentUser.Value;
}