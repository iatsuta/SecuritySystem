using System.Linq.Expressions;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.DependencyInjection;

public interface ISecurityContextInfoBuilder<TSecurityContext>
{
    ISecurityContextInfoBuilder<TSecurityContext> SetName(string name);

    ISecurityContextInfoBuilder<TSecurityContext> SetDisplayFunc(Func<TSecurityContext, string> displayFunc);

    ISecurityContextInfoBuilder<TSecurityContext> SetHierarchicalInfo(HierarchicalInfo<TSecurityContext> hierarchicalInfo);

    ISecurityContextInfoBuilder<TSecurityContext> SetHierarchicalInfo<TDirectedLink, TUndirectedLink>(
        Expression<Func<TSecurityContext, TSecurityContext?>> parentPath,
        AncestorLinkInfo<TSecurityContext, TDirectedLink> directedAncestorLinkInfo,
        AncestorLinkInfo<TSecurityContext, TUndirectedLink> undirectedAncestorLinkInfo) =>
        this.SetHierarchicalInfo(new HierarchicalInfo<TSecurityContext, TDirectedLink, TUndirectedLink>(parentPath, directedAncestorLinkInfo, undirectedAncestorLinkInfo));
}