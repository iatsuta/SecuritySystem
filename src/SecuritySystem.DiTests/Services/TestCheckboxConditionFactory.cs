using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.DiTests.DomainObjects;
using SecuritySystem.RelativeDomainPathInfo;

namespace SecuritySystem.DiTests.Services;

public class TestCheckboxConditionFactory<TDomainObject>(IRelativeDomainPathInfo<TDomainObject, Employee> pathToEmployeeInfo)
    : IFactory<Expression<Func<TDomainObject, bool>>>
{
    public Expression<Func<TDomainObject, bool>> Create()
    {
        return pathToEmployeeInfo.CreateCondition(employee => employee.TestCheckbox);
    }
}
