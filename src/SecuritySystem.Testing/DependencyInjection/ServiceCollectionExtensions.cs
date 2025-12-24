using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace SecuritySystem.Testing.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSecuritySystemTesting()
        {
            return services
                .AddSingleton<TestingUserAuthenticationService>()
                .AddSingletonFrom<ITestingUserAuthenticationService, TestingUserAuthenticationService>()
                .ReplaceScopedFrom<IRawUserAuthenticationService, TestingUserAuthenticationService>()

                .AddScoped(typeof(UserCredentialManager))

                .AddSingleton<RootAuthManager>()
                .AddSingleton(AdministratorsRoleList.Default)
                .AddSingleton(TestRootUserInfo.Default)
                .AddSingleton(typeof(ITestingEvaluator<>), typeof(TestingEvaluator<>));
        }
    }
}