using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.DependencyInjection;

public class SecurityContextInfoBuilder<TSecurityContext>(SecurityIdentity identity) : ISecurityContextInfoBuilder<TSecurityContext>
	where TSecurityContext : class, ISecurityContext
{
	private readonly List<Action<IServiceCollection>> extensions = new();

	private string name = typeof(TSecurityContext).Name;

	private Func<TSecurityContext, string>? customDisplayFunc;

	private HierarchicalInfo<TSecurityContext>? hierarchicalInfo;

	private FullAncestorLinkInfo<TSecurityContext>? fullAncestorLinkInfo;

	private IdentityInfo? customIdentityInfo;

	public ISecurityContextInfoBuilder<TSecurityContext> SetName(string newName)
	{
		this.name = newName;

		return this;
	}

	public ISecurityContextInfoBuilder<TSecurityContext> SetDisplayFunc(Func<TSecurityContext, string> displayFunc)
	{
		this.customDisplayFunc = displayFunc;

		return this;
	}

	public ISecurityContextInfoBuilder<TSecurityContext> SetIdentityPath<TSecurityContextIdent>(Expression<Func<TSecurityContext, TSecurityContextIdent>> identityPath)
		where TSecurityContextIdent : struct
	{
		this.customIdentityInfo = new IdentityInfo<TSecurityContext, TSecurityContextIdent>(identityPath);

		return this;
	}

	public ISecurityContextInfoBuilder<TSecurityContext> SetHierarchicalInfo(
		HierarchicalInfo<TSecurityContext> newHierarchicalInfo,
		FullAncestorLinkInfo<TSecurityContext> newFullAncestorLinkInfo)
	{
		this.hierarchicalInfo = newHierarchicalInfo;
		this.fullAncestorLinkInfo = newFullAncestorLinkInfo;

		return this;
	}

	public ISecurityContextInfoBuilder<TSecurityContext> AddExtension(Action<IServiceCollection> extension)
	{
		this.extensions.Add(extension);

		return this;
	}

	public void Register(IServiceCollection services)
	{
		var securityContextInfo = new SecurityContextInfo<TSecurityContext>(identity, this.name);

		services.AddSingleton(securityContextInfo);
		services.AddSingleton<SecurityContextInfo>(securityContextInfo);

		if (this.customDisplayFunc != null)
		{
			services.AddSingleton(new DisplayObjectInfo<TSecurityContext>(this.customDisplayFunc));
		}

		if (this.customIdentityInfo != null)
		{
			services.AddSingleton(this.customIdentityInfo);
		}

		if (this.hierarchicalInfo != null)
		{
			services.AddSingleton(this.hierarchicalInfo);
		}

		if (this.fullAncestorLinkInfo != null)
		{
			services.AddSingleton<FullAncestorLinkInfo>(this.fullAncestorLinkInfo);
			services.AddSingleton(this.fullAncestorLinkInfo);

			var directLinkType =
				typeof(FullAncestorLinkInfo<,>).MakeGenericType(this.fullAncestorLinkInfo.DomainObjectType, this.fullAncestorLinkInfo.DirectedLinkType);

			services.Add(ServiceDescriptor.Singleton(directLinkType, this.fullAncestorLinkInfo));
		}

		foreach (var extension in this.extensions)
		{
			extension(services);
		}
	}
}