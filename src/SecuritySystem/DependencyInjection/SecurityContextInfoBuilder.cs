using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;


namespace SecuritySystem.DependencyInjection;

public class SecurityContextInfoBuilder<TSecurityContext>(Guid id) : ISecurityContextInfoBuilder<TSecurityContext>
    where TSecurityContext : ISecurityContext
{
    private string name = typeof(TSecurityContext).Name;

    private Func<TSecurityContext, string> displayFunc = securityContext => securityContext.ToString() ?? typeof(TSecurityContext).Name;

    private HierarchicalInfo<TSecurityContext>? hierarchicalInfo;

    public ISecurityContextInfoBuilder<TSecurityContext> SetName(string newName)
    {
        this.name = newName;

        return this;
    }

    public ISecurityContextInfoBuilder<TSecurityContext> SetDisplayFunc(Func<TSecurityContext, string> newDisplayFunc)
    {
        this.displayFunc = newDisplayFunc;

        return this;
    }

    public ISecurityContextInfoBuilder<TSecurityContext> SetHierarchicalInfo(HierarchicalInfo<TSecurityContext> newHierarchicalInfo)
    {
        this.hierarchicalInfo = newHierarchicalInfo;

        return this;
    }

    public void Register(IServiceCollection services)
    {
        var securityContextInfo = new SecurityContextInfo<TSecurityContext>(id, this.name);

        services.AddSingleton(securityContextInfo);
        services.AddSingleton<SecurityContextInfo>(securityContextInfo);
        services.AddSingleton<ISecurityContextDisplayService<TSecurityContext>>(new SecurityContextDisplayService<TSecurityContext>(this.displayFunc));

        if (this.hierarchicalInfo != null)
        {
            services.AddSingleton(this.hierarchicalInfo);
        }
    }
}
