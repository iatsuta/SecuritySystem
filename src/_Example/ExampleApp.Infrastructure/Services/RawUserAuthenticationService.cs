using Microsoft.AspNetCore.Http;

using SecuritySystem.Services;

namespace ExampleApp.Infrastructure.Services;

public class RawUserAuthenticationService(IHttpContextAccessor httpContextAccessor) : IRawUserAuthenticationService
{
    public string GetUserName()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
    }
}