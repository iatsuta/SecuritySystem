using System.Linq.Expressions;

using SecuritySystem.Services;

namespace SecuritySystem.SecurityAccessor;

public class SecurityAccessorInfinityStorage<TUser>(IQueryableSource queryableSource, IVisualIdentityInfoSource visualIdentityInfoSource)
	: ISecurityAccessorInfinityStorage
	where TUser : class
{
	private readonly Expression<Func<TUser, string>> namePath = visualIdentityInfoSource.GetVisualIdentityInfo<TUser>().Name.Path;

	public IEnumerable<string> GetInfinityData() => queryableSource.GetQueryable<TUser>().Select(namePath);
}