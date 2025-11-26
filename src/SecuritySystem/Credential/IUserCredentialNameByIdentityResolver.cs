namespace SecuritySystem.Credential;

public interface IUserCredentialNameByIdentityResolver
{
    public string? TryGetUserName(SecurityIdentity identity);
}
