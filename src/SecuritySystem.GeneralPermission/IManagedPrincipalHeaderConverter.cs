using System.Linq.Expressions;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface IManagedPrincipalHeaderConverter<TPrincipal>
{
    Expression<Func<TPrincipal, ManagedPrincipalHeader>> ConvertExpression { get; }

    ManagedPrincipalHeader Convert(TPrincipal principal);
}