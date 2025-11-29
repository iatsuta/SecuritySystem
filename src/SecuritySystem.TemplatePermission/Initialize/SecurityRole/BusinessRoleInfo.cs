using System.Linq.Expressions;

namespace SecuritySystem.TemplatePermission.Initialize;

public record BusinessRoleInfo<TBusinessRole>(
	PropertyAccessors<TBusinessRole, string> Name,
	PropertyAccessors<TBusinessRole, string> Description)
{

	public BusinessRoleInfo(Expression<Func<TBusinessRole, string>> namePath,
		Expression<Func<TBusinessRole, string>> descriptionPath)
		: this(new PropertyAccessors<TBusinessRole, string>(namePath), new PropertyAccessors<TBusinessRole, string>(descriptionPath))
	{
	}
}