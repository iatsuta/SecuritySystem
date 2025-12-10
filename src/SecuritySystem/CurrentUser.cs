using SecuritySystem.Services;

namespace SecuritySystem;

public class CurrentUser(IRawUserAuthenticationService rawUserAuthenticationService, IRunAsManager? runAsManager = null)
    : RawCurrentUser(rawUserAuthenticationService)
{
    public override string Name => runAsManager?.RunAsUser?.Name ?? base.Name;
}