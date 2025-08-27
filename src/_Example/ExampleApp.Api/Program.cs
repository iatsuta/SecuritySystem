using CommonFramework.DependencyInjection;

using ExampleApp.Infrastructure.DependencyInjection;

using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;

using SecuritySystem.Configurator;

namespace ExampleApp.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddJsonFile("appSettings.json", false, true);

        builder.Host
            .UseDefaultServiceProvider(x =>
            {
                x.ValidateScopes = true;
                x.ValidateOnBuild = true;
            });

        builder
            .Services
            .AddInfrastructure(builder.Configuration)
            .AddConfigurator()

            .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        builder.Services.AddControllers(x => x.EnableEndpointRouting = false);

        builder.Services.ValidateDuplicateDeclaration(typeof(ILoggerFactory));

        var app = builder.Build();


        app
            .UseHttpsRedirection()
            .UseHsts()
            .UseAuthentication()
            .UseAuthorization()
            .UseConfigurator()
            .UseSwagger()
            .UseSwaggerUI()
            .UseRouting()
            .UseEndpoints(x => x.MapControllers());

        await app.RunAsync();
    }
}