using Microsoft.AspNetCore.Http;

namespace SecuritySystem.Configurator.Interfaces;

public interface IHandler
{
    Task Execute(HttpContext context, CancellationToken cancellationToken);
}
