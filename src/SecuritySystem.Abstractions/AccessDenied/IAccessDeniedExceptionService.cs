using SecuritySystem.Providers;

namespace SecuritySystem.AccessDenied;

public interface IAccessDeniedExceptionService
{
    Exception GetAccessDeniedException(AccessResult.AccessDeniedResult accessDeniedResult);
}
