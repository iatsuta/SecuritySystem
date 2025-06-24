using CommonFramework;

using Microsoft.AspNetCore.Http;

using SecuritySystem.Configurator.Interfaces;
using SecuritySystem.Services;

namespace SecuritySystem.Configurator.Handlers;

public class RunAsHandler(IRunAsManager? runAsManager = null) : BaseWriteHandler, IRunAsHandler
{
    public async Task Execute(HttpContext context, CancellationToken cancellationToken) =>
        await runAsManager.FromMaybe(() => "RunAs not supported")
                          .StartRunAsUserAsync(await this.ParseRequestBodyAsync<string>(context), cancellationToken);
}
