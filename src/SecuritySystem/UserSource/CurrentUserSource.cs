using CommonFramework;

namespace SecuritySystem.UserSource;

public class CurrentUserSource<TUser>(ICurrentUser currentUser, IUserSource<TUser> userSource) : ICurrentUserSource<TUser>
{
    private readonly Lazy<TUser> lazyCurrentUser = LazyHelper.Create(() => userSource.GetUser(currentUser.Name));

    public TUser CurrentUser => this.lazyCurrentUser.Value;
}