using System.Reflection;

using CommonFramework;
using CommonFramework.RelativePath;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.DependencyInjection.DomainSecurityServiceBuilder;

internal class DomainSecurityServiceRootBuilder : IDomainSecurityServiceRootBuilder
{
    private readonly List<DomainSecurityServiceBuilder> domainBuilders = [];

    public bool AutoAddSelfRelativePath { get; set; } = true;

    public IDomainSecurityServiceRootBuilder Add<TDomainObject>(Action<IDomainSecurityServiceBuilder<TDomainObject>> setup)
    {
        return new Func<Action<IDomainSecurityServiceBuilder<object>>, IDomainSecurityServiceRootBuilder>(this.AddInternal)
               .CreateGenericMethod(typeof(TDomainObject))
               .Invoke<IDomainSecurityServiceRootBuilder>(this, setup);
    }

    public IDomainSecurityServiceRootBuilder AddMetadata<TMetadata>()
        where TMetadata : IDomainSecurityServiceMetadata
    {
        return this.GetType().GetMethod(nameof(this.AddMetadataInternal), BindingFlags.Instance | BindingFlags.NonPublic)!
                   .MakeGenericMethod(typeof(TMetadata), TMetadata.DomainType)
                   .Invoke<IDomainSecurityServiceRootBuilder>(this);
    }

    private IDomainSecurityServiceRootBuilder AddMetadataInternal<TMetadata, TDomainObject>()
        where TMetadata : IDomainSecurityServiceMetadata<TDomainObject>
    {
        return this.AddInternal<TDomainObject>(b => b.Override<TMetadata>().Pipe(TMetadata.Setup));
    }

    private IDomainSecurityServiceRootBuilder AddInternal<TDomainObject>(Action<IDomainSecurityServiceBuilder<TDomainObject>> setup)
    {
        var builder = new DomainSecurityServiceBuilder<TDomainObject>();

        setup(builder);

        this.domainBuilders.Add(builder);

        return this;
    }

    public void Register(IServiceCollection services)
    {
        foreach (var domainBuilder in this.domainBuilders)
        {
            domainBuilder.Register(services);

            if (this.AutoAddSelfRelativePath)
            {
                services.AddSingleton(
                    typeof(IRelativeDomainPathInfo<,>).MakeGenericType(domainBuilder.DomainType, domainBuilder.DomainType),
                    typeof(SelfRelativeDomainPathInfo<>).MakeGenericType(domainBuilder.DomainType));
            }
        }
    }
}
