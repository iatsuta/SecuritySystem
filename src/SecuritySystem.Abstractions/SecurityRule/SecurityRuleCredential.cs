using SecuritySystem.Credential;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public abstract record SecurityRuleCredential
{
    public record CurrentUserWithRunAsCredential : SecurityRuleCredential;

    public record CurrentUserWithoutRunAsCredential : SecurityRuleCredential;

    public record AnyUserCredential : SecurityRuleCredential;

    public record CustomUserSecurityRuleCredential(UserCredential UserCredential) : SecurityRuleCredential;
}
