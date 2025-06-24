using CommonFramework;

using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem;

public class CurrentUser : ICurrentUser
{
    private readonly IRunAsManager? runAsManager;

    private readonly Lazy<string> lazyName;

    private readonly Lazy<Guid> lazyId;

    public CurrentUser(
        IRawUserAuthenticationService rawUserAuthenticationService,
        IRunAsManager? runAsManager = null,
        IUserSource? userSource = null)
    {
        this.runAsManager = runAsManager;
        this.lazyName = LazyHelper.Create(rawUserAuthenticationService.GetUserName);

        this.lazyId = LazyHelper.Create(
            () => (userSource ?? throw new UserSourceException($"{nameof(UserSource)} not defined")).GetUser(this.Name).Id);
    }

    public Guid Id => this.lazyId.Value;

    public string Name => this.runAsManager?.RunAsUser?.Name ?? this.lazyName.Value;
}
