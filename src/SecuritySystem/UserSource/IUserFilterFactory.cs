using System.Linq.Expressions;

using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public interface IUserFilterFactory<TUser>
{
	Expression<Func<TUser, bool>> CreateFilter(UserCredential userCredential);
}