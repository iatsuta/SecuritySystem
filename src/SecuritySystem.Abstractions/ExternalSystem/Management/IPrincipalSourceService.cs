namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalSourceService : IPrincipalSourceServiceBase
{
    Type PrincipalType { get; }
}