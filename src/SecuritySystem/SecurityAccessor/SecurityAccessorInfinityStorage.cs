using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.SecurityAccessor;

public class SecurityAccessorInfinityStorage<TUser>(IQueryableSource queryableSource, UserSourceInfo<TUser> userSourceInfo) : ISecurityAccessorInfinityStorage
	where TUser : class
{
	public IEnumerable<string> GetInfinityData() => queryableSource.GetQueryable<TUser>().Select(userSourceInfo.Name.Path);
}