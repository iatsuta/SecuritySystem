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
        return credential switch
        {
            SecurityRuleCredential.CustomUserSecurityRuleCredential customUserSecurityRuleCredential => simpleUserSource
                .TryGetUserAsync(customUserSecurityRuleCredential.UserCredential)
                .GetAwaiter()
                .GetResult()
                ?.Name,

            SecurityRuleCredential.CurrentUserWithRunAsCredential => currentUser.Name,

            SecurityRuleCredential.CurrentUserWithoutRunAsCredential => rawUserAuthenticationService.GetUserName(),

            SecurityRuleCredential.AnyUserCredential => null,

            _ => throw new ArgumentOutOfRangeException(nameof(credential))
        };
    }
}
