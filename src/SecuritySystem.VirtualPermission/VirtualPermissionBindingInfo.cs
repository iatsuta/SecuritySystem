using CommonFramework;

using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;

namespace SecuritySystem.VirtualPermission;

public record VirtualPermissionBindingInfo<TPrincipal, TPermission>(SecurityRole SecurityRole) : PermissionBindingInfo<TPermission, TPrincipal>
    where TPermission : notnull
{
	public IReadOnlyList<LambdaExpression> Restrictions { get; init; } = [];

	public Func<IServiceProvider, Expression<Func<TPermission, bool>>> GetFilter { get; init; } = _ => _ => true;

	public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddRestriction<TSecurityContext>(
		Expression<Func<TPermission, IEnumerable<TSecurityContext>>> path)
		where TSecurityContext : ISecurityContext =>

		this with { Restrictions = this.Restrictions.Concat([path]).ToList() };

	public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddRestriction<TSecurityContext>(
		Expression<Func<TPermission, TSecurityContext?>> path)
		where TSecurityContext : ISecurityContext =>

		this with { Restrictions = this.Restrictions.Concat([path]).ToList() };

	public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddFilter(
		Expression<Func<TPermission, bool>> filter) => this.AddFilter(_ => filter);

	public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddFilter(
		Func<IServiceProvider, Expression<Func<TPermission, bool>>> getFilter) =>

		this with { GetFilter = sp => this.GetFilter(sp).BuildAnd(getFilter(sp)) };

	public VirtualPermissionBindingInfo<TPrincipal, TPermission> SetPeriodFilter(
		Expression<Func<TPermission, PermissionPeriod>> periodFilter) =>

		this with { PermissionPeriod = periodFilter.ToPropertyAccessors() };

	public VirtualPermissionBindingInfo<TPrincipal, TPermission> SetComment(
		Expression<Func<TPermission, string>> commentPath) =>

		this with { PermissionComment = commentPath.ToPropertyAccessors() };

	public IEnumerable<Type> GetSecurityContextTypes()
	{
		return this.Restrictions
			.Select(restrictionPath => restrictionPath.ReturnType.GetCollectionElementTypeOrSelf())
			.Distinct();
	}

	public Expression<Func<TPermission, Array>> GetRestrictionsArrayExpr(IdentityInfo identityInfo, LambdaExpression? pureFilter)
	{
		return new Func<IdentityInfo<ISecurityContext, Ignore>, Expression<Func<ISecurityContext, bool>>?, Expression<Func<TPermission, Array>>>(
				this.GetRestrictionsArrayExpr)
			.CreateGenericMethod(identityInfo.DomainObjectType, identityInfo.IdentityType)
			.Invoke<Expression<Func<TPermission, Array>>>(this, identityInfo, pureFilter);
	}

	public Expression<Func<TPermission, Array>> GetRestrictionsArrayExpr<TSecurityContext, TSecurityContextIdent>(
		IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo,
		Expression<Func<TSecurityContext, bool>>? pureFilter)
		where TSecurityContext : ISecurityContext
		where TSecurityContextIdent : notnull
	{
		return from idents in this.GetRestrictionsExpr(identityInfo, pureFilter)

			select (Array)idents.ToArray();
	}

	public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(
		IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo,
		Expression<Func<TSecurityContext, bool>>? pureFilter)
		where TSecurityContext : ISecurityContext
		where TSecurityContextIdent : notnull
	{
		var expressions = this.GetManyRestrictionsExpr(identityInfo, pureFilter);

		return expressions.Match(
			() => _ => Array.Empty<TSecurityContextIdent>(),
			single => single,
			many => many.Aggregate((state, expr) =>
				from ids1 in state
				from ide2 in expr
				select ids1.Concat(ide2)));
	}

	private IEnumerable<Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>>> GetManyRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(
		IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo, Expression<Func<TSecurityContext, bool>>? pureFilter)
		where TSecurityContext : ISecurityContext
		where TSecurityContextIdent : notnull
	{
		foreach (var restrictionPath in this.Restrictions)
		{
			if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
			{
				yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
				{
					if (pureFilter == null)
					{
						return singlePath.Select(IEnumerable<TSecurityContextIdent> (securityContext) =>
							securityContext != null ? new[] { ee.Evaluate(identityInfo.Id.Path, securityContext) } : Array.Empty<TSecurityContextIdent>());
					}
					else
					{
						return singlePath.Select(IEnumerable<TSecurityContextIdent> (securityContext) =>
							securityContext != null && ee.Evaluate(pureFilter, securityContext)
								? new[] { ee.Evaluate(identityInfo.Id.Path, securityContext) }
								: Array.Empty<TSecurityContextIdent>());
					}
				});
			}
			else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
			{
				yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
				{
					if (pureFilter == null)
					{
						return manyPath.Select(securityContexts =>
							securityContexts.Select(securityContext => ee.Evaluate(identityInfo.Id.Path, securityContext)));
					}
					else
					{
						return manyPath.Select(securityContexts => securityContexts
							.Where(securityContext => ee.Evaluate(pureFilter, securityContext))
							.Select(securityContext => ee.Evaluate(identityInfo.Id.Path, securityContext)));
					}
				});
			}
		}
	}
}