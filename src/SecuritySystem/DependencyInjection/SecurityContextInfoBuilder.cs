using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.DependencyInjection;

public class SecurityContextInfoBuilder<TSecurityContext>(Guid id) : ISecurityContextInfoBuilder<TSecurityContext>
    where TSecurityContext : ISecurityContext
{
    private string name = typeof(TSecurityContext).Name;

    private Func<TSecurityContext, string> displayFunc = securityContext => securityContext.ToString() ?? typeof(TSecurityContext).Name;

    private HierarchicalInfo<TSecurityContext>? hierarchicalInfo;

    private IdentityInfo? customIdentityInfo;

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

    public ISecurityContextInfoBuilder<TSecurityContext> SetIdentityPath<TIdent>(Expression<Func<TSecurityContext, TIdent>> identityPath)
        where TIdent : struct
    {
        this.customIdentityInfo = new IdentityInfo<TSecurityContext, TIdent>(identityPath);

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

        if (this.customIdentityInfo != null)
        {
            services.AddSingleton(this.customIdentityInfo);
        }

        if (this.hierarchicalInfo != null)
        {
            services.AddSingleton<HierarchicalInfo>(this.hierarchicalInfo);
            services.AddSingleton<HierarchicalInfo<TSecurityContext>>(this.hierarchicalInfo);

            var directLinkType = typeof(HierarchicalInfo<,>).MakeGenericType(this.hierarchicalInfo.DomainObjectType, this.hierarchicalInfo.DirectedLinkType);

            services.Add(ServiceDescriptor.Singleton(directLinkType, this.hierarchicalInfo));
        }
    }
}
