using CommonFramework;

using SecuritySystem.Credential;

using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserSourceRunAsManager<TUser>(
    IRawUserAuthenticationService rawUserAuthenticationService,
    ISecuritySystemFactory securitySystemFactory,
    IUserSource<TUser> userSource,
    IUserSourceRunAsAccessor<TUser> accessor,
    UserPathInfo<TUser> userPathInfo,
    IGenericRepository genericRepository) : RunAsManager(rawUserAuthenticationService, securitySystemFactory)
    where TUser : class
{
    private readonly Lazy<Func<TUser, User>> lazyToDefaultUserFunc = LazyHelper.Create(() => userPathInfo.ToDefaultUserExpr.Compile());

    private readonly TUser currentUser = userSource.GetUser(rawUserAuthenticationService.GetUserName());

    public override User? RunAsUser => this.NativeRunAsUser.Maybe(v => this.lazyToDefaultUserFunc.Value(v));

    private TUser? NativeRunAsUser => accessor.GetRunAs(this.currentUser);

    protected override async Task PersistRunAs(UserCredential? userCredential, CancellationToken cancellationToken)
    {
        var newRunAsUser = userCredential == null ? null : userSource.GetUser(userCredential);

        if (this.NativeRunAsUser != newRunAsUser)
        {
            accessor.SetRunAs(this.currentUser, newRunAsUser);

            await genericRepository.SaveAsync(this.currentUser, cancellationToken);
        }
    }
}