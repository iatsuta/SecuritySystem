using SecuritySystem.Services;

namespace ExampleWebApp.Infrastructure.Services;

public class RawUserAuthenticationService(IHttpContextAccessor httpContextAccessor) : IRawUserAuthenticationService
{
    public string GetUserName()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
    }
}