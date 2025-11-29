using System.Linq.Expressions;

namespace SecuritySystem.TemplatePermission.Initialize;

public record SecurityRoleInfo<TSecurityRole>(
	PropertyAccessors<TSecurityRole, string> Name,
	PropertyAccessors<TSecurityRole, string> Description)
{

	public SecurityRoleInfo(Expression<Func<TSecurityRole, string>> namePath,
		Expression<Func<TSecurityRole, string>> descriptionPath)
		: this(new PropertyAccessors<TSecurityRole, string>(namePath), new PropertyAccessors<TSecurityRole, string>(descriptionPath))
	{
	}
}