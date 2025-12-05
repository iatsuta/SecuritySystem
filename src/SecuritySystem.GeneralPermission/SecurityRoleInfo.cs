using System.Linq.Expressions;
using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record SecurityRoleInfo<TSecurityRole>(PropertyAccessors<TSecurityRole, string> Name, PropertyAccessors<TSecurityRole, string> Description)
{
	public SecurityRoleInfo(Expression<Func<TSecurityRole, string>> namePath,
		Expression<Func<TSecurityRole, string>> descriptionPath)
		: this(new PropertyAccessors<TSecurityRole, string>(namePath), new PropertyAccessors<TSecurityRole, string>(descriptionPath))
	{
	}
}