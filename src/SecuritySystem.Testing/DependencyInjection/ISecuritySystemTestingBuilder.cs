namespace SecuritySystem.Testing.DependencyInjection;

public interface ISecuritySystemTestingBuilder
{
    ISecuritySystemTestingBuilder SetUserAuthenticationService<TTestingUserAuthenticationService>(bool replaceScoped = true)
        where TTestingUserAuthenticationService : class, ITestingUserAuthenticationService;

    ISecuritySystemTestingBuilder SetEvaluator(Type evaluatorType);

    ISecuritySystemTestingBuilder SetTestRootUserInfo(Func<IServiceProvider, TestRootUserInfo> getInfo);
}