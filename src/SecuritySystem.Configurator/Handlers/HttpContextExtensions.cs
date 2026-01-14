using Microsoft.AspNetCore.Http;

namespace SecuritySystem.Configurator.Handlers;

internal static class HttpContextExtensions
{
    extension(HttpContext httpContent)
    {
        public SecurityIdentity ExtractSecurityIdentity()
        {
            return new UntypedSecurityIdentity((string)httpContent.Request.RouteValues["id"]!);
        }

        public string ExtractName()
        {
            return (string)httpContent.Request.RouteValues["name"]!;
        }

        public string ExtractSearchToken()
        {
            return httpContent.Request.Query["searchToken"]!;
        }
    }
}