using CommonFramework;

using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem;

public class CurrentUser : ICurrentUser
{
    private readonly IRunAsManager? runAsManager;

    private readonly Lazy<string> lazyRawName;

    private readonly Lazy<SecurityIdentity> lazyIdentity;

    public CurrentUser(
        IRawUserAuthenticationService rawUserAuthenticationService,
        IRunAsManager? runAsManager = null,
        IUserSource<User>? userSource = null)
    {
        this.runAsManager = runAsManager;
        this.lazyRawName = LazyHelper.Create(rawUserAuthenticationService.GetUserName);

        this.lazyIdentity = LazyHelper.Create(
            () => (userSource ?? throw new UserSourceException($"{nameof(UserSource)} not defined")).GetUser(this.Name).Identity);
    }

    public SecurityIdentity Identity => this.lazyIdentity.Value;

    public string Name => this.runAsManager?.RunAsUser?.Name ?? this.lazyRawName.Value;
}
