using CommonFramework;

using SecuritySystem.Services;

namespace SecuritySystem;

public class RawCurrentUser(IRawUserAuthenticationService rawUserAuthenticationService) : ICurrentUser
{
    private readonly Lazy<string> lazyRawName = LazyHelper.Create(rawUserAuthenticationService.GetUserName);

    public virtual string Name => this.lazyRawName.Value;
}