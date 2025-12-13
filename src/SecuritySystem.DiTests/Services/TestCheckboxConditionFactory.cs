using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.RelativePath;

using SecuritySystem.DiTests.DomainObjects;

namespace SecuritySystem.DiTests.Services;

public class TestCheckboxConditionFactory<TDomainObject>(IRelativeDomainPathInfo<TDomainObject, Employee> pathToEmployeeInfo)
    : IFactory<Expression<Func<TDomainObject, bool>>>
{
    public Expression<Func<TDomainObject, bool>> Create()
    {
        return pathToEmployeeInfo.CreateCondition(employee => employee.TestCheckbox);
    }
}
