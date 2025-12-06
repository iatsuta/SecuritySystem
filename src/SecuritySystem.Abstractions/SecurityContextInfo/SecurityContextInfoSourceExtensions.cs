// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public static class SecurityContextInfoSourceExtensions
{
	extension(ISecurityContextInfoSource securityContextInfoSource)
	{
		public IEnumerable<Type> GetSecurityContextTypes() =>
			securityContextInfoSource.SecurityContextInfoList.Select(info => info.Type);

		public SecurityContextInfo GetSecurityContextInfo<TSecurityContext>()
			where TSecurityContext : ISecurityContext =>
			securityContextInfoSource.GetSecurityContextInfo(typeof(TSecurityContext));
	}
}