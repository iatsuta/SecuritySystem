using System.Text.Json;

using Microsoft.AspNetCore.Http;

using SecuritySystem.Configurator.Interfaces;

namespace SecuritySystem.Configurator.Handlers;

public abstract class BaseReadHandler : IHandler
{
    public async Task Execute(HttpContext context, CancellationToken cancellationToken)
    {
        var data = await this.GetDataAsync(context, cancellationToken);
        await context.Response.WriteAsync(JsonSerializer.Serialize(data), cancellationToken);
    }

    protected abstract Task<object> GetDataAsync(HttpContext context, CancellationToken cancellationToken);
}
