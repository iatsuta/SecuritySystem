using System.Linq.Expressions;

namespace SecuritySystem.UserSource;

public record UserSourceRunAsAccessorData<TUser>(Expression<Func<TUser, TUser?>> Path);
