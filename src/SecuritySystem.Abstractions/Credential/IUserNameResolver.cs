namespace SecuritySystem.Credential;

public interface IUserNameResolver<out TUser>
{
    string? Resolve(SecurityRuleCredential credential);
}
