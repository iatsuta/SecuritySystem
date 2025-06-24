// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface IUserNameResolver
{
    string? Resolve(SecurityRuleCredential credential);
}
