// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecurityContextDisplayService<in TSecurityContext>
{
    string ToString(TSecurityContext securityContext);
}
