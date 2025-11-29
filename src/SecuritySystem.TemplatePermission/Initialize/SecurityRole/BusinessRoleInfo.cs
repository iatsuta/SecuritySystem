using System.Linq.Expressions;

namespace SecuritySystem.TemplatePermission.Initialize;

public record BusinessRoleInfo<TBusinessRole>(
	Expression<Func<TBusinessRole, string>> NamePath,
	Expression<Func<TBusinessRole, string>> DescriptionPath)
{
	public PropertyAccessors<TBusinessRole, string> NameAccessors { get; } = new(NamePath);

	public PropertyAccessors<TBusinessRole, string> DescriptionAccessors { get; } = new(DescriptionPath);
}