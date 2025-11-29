using SecuritySystem.Services;
using SecuritySystem.UserSource;

// ReSharper disable once CheckNamespace
namespace SecuritySystem.Credential;

public class UserNameResolver<TUser>(
    ICurrentUser currentUser,
    IRawUserAuthenticationService rawUserAuthenticationService,
    IUserSource<TUser> userSource) : IUserNameResolver<TUser>
{
	private readonly IUserSource<User> simpleUserSource = userSource.ToSimple();

	public string? Resolve(SecurityRuleCredential credential)
    {
        switch (credential)
        {
            case SecurityRuleCredential.CustomUserSecurityRuleCredential customUserSecurityRuleCredential:
	            return simpleUserSource.TryGetUserAsync(customUserSecurityRuleCredential.UserCredential).GetAwaiter().GetResult()?.Name;

            case SecurityRuleCredential.CurrentUserWithRunAsCredential:
                return currentUser.Name;

            case SecurityRuleCredential.CurrentUserWithoutRunAsCredential:
                return rawUserAuthenticationService.GetUserName();

            case SecurityRuleCredential.AnyUserCredential:
                return null;

            default:
                throw new ArgumentOutOfRangeException(nameof(credential));
        }
    }
}
