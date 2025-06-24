using SecuritySystem.Credential;
using SecuritySystem.Services;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public class UserNameResolver(
    ICurrentUser currentUser,
    IRawUserAuthenticationService rawUserAuthenticationService,
    IUserCredentialNameResolver userCredentialNameResolver) : IUserNameResolver
{
    public string? Resolve(SecurityRuleCredential credential)
    {
        switch (credential)
        {
            case SecurityRuleCredential.CustomUserSecurityRuleCredential customUserSecurityRuleCredential:
                return userCredentialNameResolver.GetUserName(customUserSecurityRuleCredential.UserCredential);

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
