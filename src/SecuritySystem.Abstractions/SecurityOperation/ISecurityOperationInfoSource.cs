// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface ISecurityOperationInfoSource
{
    SecurityOperationInfo GetSecurityOperationInfo(SecurityOperation securityOperation);
}
