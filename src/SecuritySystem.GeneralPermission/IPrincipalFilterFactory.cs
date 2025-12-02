using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPrincipalFilterFactory<TPrincipal>
{
	Expression<Func<TPrincipal, bool>> CreateFilterById(string principalId);
}