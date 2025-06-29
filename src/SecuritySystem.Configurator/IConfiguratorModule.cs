﻿using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Configurator;

public interface IConfiguratorModule
{
    string Name { get; }

    void AddServices(IServiceCollection services);

    void MapApi(IEndpointRouteBuilder endpointsBuilder, string route);
}
