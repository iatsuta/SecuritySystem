using CommonFramework;

namespace SecuritySystem.UserSource;

public class UserSourceRunAsAccessor<TUser>(UserSourceRunAsAccessorData<TUser> data) : IUserSourceRunAsAccessor<TUser>
{
    private readonly Func<TUser, TUser?> getRunAsFunc = data.Path.Compile();

    private readonly Action<TUser, TUser?> setRunAsAction = data.Path.ToSetLambdaExpression().Compile();

    public TUser? GetRunAs(TUser user) => this.getRunAsFunc(user);

    public void SetRunAs(TUser user, TUser? targetUser) => this.setRunAsAction(user, targetUser);
}