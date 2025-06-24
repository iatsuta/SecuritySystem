using CommonFramework;

using SecuritySystem.Credential;
using SecuritySystem.PersistStorage;

using SecuritySystem.Services;

namespace SecuritySystem.UserSource;

public class UserSourceRunAsManager<TUser>(
    IRawUserAuthenticationService rawUserAuthenticationService,
    ISecuritySystemFactory securitySystemFactory,
    IUserSource<TUser> userSource,
    IUserSourceRunAsAccessor<TUser> accessor,
    UserPathInfo<TUser> userPathInfo,
    IPersistStorage<TUser> persistStorage) : RunAsManager(rawUserAuthenticationService, securitySystemFactory)
{
    private readonly Lazy<Func<TUser, User>> lazyToDefaultUserFunc = LazyHelper.Create(() => userPathInfo.ToDefaultUserExpr.Compile());

    private readonly TUser currentUser = userSource.GetUser(rawUserAuthenticationService.GetUserName());

    public override User? RunAsUser => accessor.GetRunAs(this.currentUser).Maybe(v => this.lazyToDefaultUserFunc.Value(v));

    protected override async Task PersistRunAs(UserCredential? userCredential, CancellationToken cancellationToken)
    {
        var runAsUser = userCredential == null ? default : userSource.GetUser(userCredential);

        accessor.SetRunAs(this.currentUser, runAsUser);

        await persistStorage.SaveAsync(this.currentUser, cancellationToken);
    }
}