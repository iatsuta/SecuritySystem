namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public interface ISecurityContextStorage
{
    ITypedSecurityContextStorage GetTyped(Type securityContextType);
}