using System.Reflection;

using CommonFramework;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using SecuritySystem.Configurator.Interfaces;

namespace SecuritySystem.Configurator;

public static class ConfiguratorDependencyInjection
{
    private const string EmbeddedFileNamespace = $"{nameof(SecuritySystem)}.{nameof(Configurator)}.configurator_ui.dist";

    public static IServiceCollection AddConfigurator(this IServiceCollection services, Action<IConfiguratorSetup>? setupAction = null)
    {
        var configurationSetup = new ConfiguratorSetup();

        configurationSetup.AddModule(new ConfiguratorMainModule());
        setupAction?.Invoke(configurationSetup);

        configurationSetup.Initialize(services);

        return services;
    }

    public static IApplicationBuilder UseConfigurator(this IApplicationBuilder app, string route = "/admin/configurator") =>
        app
            .UseMiddleware<ConfiguratorMiddleware>(route)
            .UseStaticFiles(
                new StaticFileOptions
                {
                    RequestPath = route,
                    FileProvider = new EmbeddedFileProvider(
                        typeof(ConfiguratorDependencyInjection).GetTypeInfo().Assembly,
                        EmbeddedFileNamespace)
                })
            .UseRouting() // needed for IIS
            .UseEndpoints(x => x.MapApi(route));

    extension(IEndpointRouteBuilder endpointsBuilder)
    {
        private void MapApi(string route) =>
            endpointsBuilder.ServiceProvider.GetRequiredService<IEnumerable<IConfiguratorModule>>()
                .Foreach(module => module.MapApi(endpointsBuilder, route));

        public IEndpointRouteBuilder Get<THandler>(string pattern)
            where THandler : IHandler
        {
            endpointsBuilder.MapGet(pattern, async x => await x.RequestServices.GetRequiredService<THandler>().Execute(x, x.RequestAborted));
            return endpointsBuilder;
        }

        public IEndpointRouteBuilder Post<THandler>(string pattern)
            where THandler : IHandler
        {
            endpointsBuilder.MapPost(pattern, async x => await x.RequestServices.GetRequiredService<THandler>().Execute(x, x.RequestAborted));
            return endpointsBuilder;
        }

        public IEndpointRouteBuilder Delete<THandler>(string pattern)
            where THandler : IHandler
        {
            endpointsBuilder.MapDelete(pattern, async x => await x.RequestServices.GetRequiredService<THandler>().Execute(x, x.RequestAborted));
            return endpointsBuilder;
        }
    }
}
