using CommonFramework;

using SecuritySystem.Services;

namespace SecuritySystem;

public class CurrentUser(IRawUserAuthenticationService rawUserAuthenticationService, IRunAsManager? runAsManager = null) : ICurrentUser
{
	private readonly Lazy<string> lazyRawName = LazyHelper.Create(rawUserAuthenticationService.GetUserName);

    public string Name => runAsManager?.RunAsUser?.Name ?? this.lazyRawName.Value;
}
