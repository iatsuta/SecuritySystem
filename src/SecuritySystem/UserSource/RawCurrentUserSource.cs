using SecuritySystem.Attributes;

namespace SecuritySystem.UserSource;

public class RawCurrentUserSource<TUser>(ICurrentUser currentUser, [WithoutRunAs] IUserSource<TUser> userSource)
    : CurrentUserSource<TUser>(currentUser, userSource);