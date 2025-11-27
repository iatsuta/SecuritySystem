using System.Linq.Expressions;
using SecuritySystem.UserSource;

namespace SecuritySystem.Services;

public interface IDefaultUserConverter<TUser>
{
	Expression<Func<TUser, User>> GetConvertFunc();
}