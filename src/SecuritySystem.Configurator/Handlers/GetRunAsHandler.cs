using Microsoft.AspNetCore.Http;

using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Services;

namespace SecuritySystem.Configurator.Handlers;

public class GetRunAsHandler(IRunAsManager? runAsManager = null) : BaseReadHandler, IGetRunAsHandler
{
    protected override async Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken) =>
        runAsManager?.RunAsUser?.Name ?? string.Empty;
}
