using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.DependencyInjection;

public interface ISecurityContextInfoBuilder<TSecurityContext>
{
    ISecurityContextInfoBuilder<TSecurityContext> SetName(string name);

    ISecurityContextInfoBuilder<TSecurityContext> SetDisplayFunc(Func<TSecurityContext, string> displayFunc);

    ISecurityContextInfoBuilder<TSecurityContext> SetIdentityPath<TIdent>(Expression<Func<TSecurityContext, TIdent>> identityPath)
        where TIdent : struct;

    ISecurityContextInfoBuilder<TSecurityContext> SetHierarchicalInfo(
        HierarchicalInfo<TSecurityContext> hierarchicalInfo,
        FullAncestorLinkInfo<TSecurityContext> fullAncestorLinkInfo);

    ISecurityContextInfoBuilder<TSecurityContext> SetHierarchicalInfo<TDirectedLink, TUndirectedLink>(
        Expression<Func<TSecurityContext, TSecurityContext?>> parentPath,
        AncestorLinkInfo<TSecurityContext, TDirectedLink> directed,
        AncestorLinkInfo<TSecurityContext, TUndirectedLink> undirected) =>
        this.SetHierarchicalInfo(
            new HierarchicalInfo<TSecurityContext>(parentPath),
            new FullAncestorLinkInfo<TSecurityContext, TDirectedLink, TUndirectedLink>(directed, undirected));

    ISecurityContextInfoBuilder<TSecurityContext> AddExtension(Action<IServiceCollection> extension);
}