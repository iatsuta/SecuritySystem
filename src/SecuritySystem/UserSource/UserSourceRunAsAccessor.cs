using CommonFramework;

namespace SecuritySystem.UserSource;

public class UserSourceRunAsAccessor<TUser> : IUserSourceRunAsAccessor<TUser>
{
    private readonly Func<TUser, TUser?> getRunAsFunc;

    private readonly Action<TUser, TUser?> setRunAsAction;

    public UserSourceRunAsAccessor(UserSourceRunAsAccessorData<TUser> data)
    {
        var property = data.Path.GetProperty();

        this.getRunAsFunc = property.GetGetValueFunc<TUser, TUser?>();
        this.setRunAsAction = property.GetSetValueAction<TUser, TUser?>();
    }

    public TUser? GetRunAs(TUser user) => this.getRunAsFunc(user);

    public void SetRunAs(TUser user, TUser? targetUser) => this.setRunAsAction(user, targetUser);
}
