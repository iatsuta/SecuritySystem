using CommonFramework;

using Microsoft.AspNetCore.Http;

using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Services;

namespace SecuritySystem.Configurator.Handlers;

public class StopRunAsHandler(IRunAsManager? runAsManager = null) : BaseWriteHandler, IStopRunAsHandler
{
    public async Task Execute(HttpContext context, CancellationToken cancellationToken) =>
        await runAsManager.FromMaybe(() => "RunAs not supported").FinishRunAsUserAsync(cancellationToken);
}
