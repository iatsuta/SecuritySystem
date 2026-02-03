using System.Linq.Expressions;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public interface IManagedPrincipalHeaderConverter<TPrincipal>
{
    Expression<Func<TPrincipal, ManagedPrincipalHeader>> ConvertExpression { get; }

    ManagedPrincipalHeader Convert(TPrincipal principal);
}