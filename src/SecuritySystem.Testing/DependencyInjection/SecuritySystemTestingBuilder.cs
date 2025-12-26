using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace SecuritySystem.Testing.DependencyInjection;

public class SecuritySystemTestingBuilder : ISecuritySystemTestingBuilder
{
    private Type testingUserAuthenticationServiceType = typeof(TestingUserAuthenticationService);

    private bool replaceScopedUserAuthenticationServiceType = true;

    private Type evaluatorType = typeof(TestingEvaluator<>);

    private Func<IServiceProvider, TestRootUserInfo> getTestRootUserInfoFunc = _ => TestRootUserInfo.Default;

    public ISecuritySystemTestingBuilder SetUserAuthenticationService<TTestingUserAuthenticationService>(bool replaceScoped = true)
        where TTestingUserAuthenticationService : class, ITestingUserAuthenticationService
    {
        this.testingUserAuthenticationServiceType = typeof(TTestingUserAuthenticationService);
        this.replaceScopedUserAuthenticationServiceType = replaceScoped;

        return this;
    }

    public ISecuritySystemTestingBuilder SetEvaluator(Type newEvaluatorType)
    {
        this.evaluatorType = newEvaluatorType;

        return this;
    }

    public ISecuritySystemTestingBuilder SetTestRootUserInfo(Func<IServiceProvider, TestRootUserInfo> getInfo)
    {
        this.getTestRootUserInfoFunc = getInfo;

        return this;
    }

    public void Initialize(IServiceCollection services)
    {
        var f = (IServiceProvider sp) => (ITestingUserAuthenticationService)sp.GetRequiredService(this.testingUserAuthenticationServiceType);

        services
            .AddSingleton(this.testingUserAuthenticationServiceType)
            .AddSingletonFrom(f);

        if (this.replaceScopedUserAuthenticationServiceType)
        {
            services
                .ReplaceScopedFrom<IRawUserAuthenticationService>(f)
                .ReplaceScopedFrom<IImpersonateService>(f);
        }

        services.AddScoped(typeof(UserCredentialManager))

            .AddSingleton<RootAuthManager>()
            .AddSingleton(AdministratorsRoleList.Default)
            .AddSingleton(this.getTestRootUserInfoFunc)
            .AddSingleton(typeof(ITestingEvaluator<>), this.evaluatorType);
    }
}