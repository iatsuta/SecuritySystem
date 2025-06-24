using SecuritySystem.Providers;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public interface IAccessDeniedExceptionService
{
    Exception GetAccessDeniedException(AccessResult.AccessDeniedResult accessDeniedResult);
}
