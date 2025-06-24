namespace SecuritySystem.ExternalSystem.SecurityContextStorage;

public interface ISecurityContextStorage
{
    ITypedSecurityContextStorage GetTyped(Guid securityContextTypeId);

    ITypedSecurityContextStorage GetTyped(Type securityContextType);
}
