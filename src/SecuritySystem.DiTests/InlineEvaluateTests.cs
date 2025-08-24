using CommonFramework;
using CommonFramework.ExpressionComparers;
using SecuritySystem.DiTests.DomainObjects;
using SecuritySystem.ExpressionEvaluate;
using System.Linq.Expressions;

namespace SecuritySystem.DiTests;

public class InlineEvaluateTests
{
    [Fact]
    public void InlineEvaluate_WhenSecondParameterIsConstant_RewritesToSimplifiedUnaryExpression()
    {
        // Arrange
        Expression<Func<int, int, int>> testExpression = (x, y) => x + y;

        Expression<Func<int, int>> expectedResult = x => x + 1;

        // Act
        var result = ExpressionEvaluateHelper.InlineEvaluate(Expression<Func<int, int>> (ee) => x => ee.Evaluate(testExpression, x, 1));

        // Assert
        result.Should().Be(expectedResult, ExpressionComparer.Value);
    }
    
    [Fact]
    public void InlineEvaluate_EmployeeBusinessUnitIdPath_TransformsToExpectedEnumerableExpression()
    {
        // Arrange
        Expression<Func<Employee, BusinessUnit?>> singlePath = employee => employee.BusinessUnit;
        Expression<Func<BusinessUnit, Guid>> idPath = bu => bu.Id;

        Expression<Func<Employee, IEnumerable<Guid>>> expectedResult = employee =>
            employee.BusinessUnit != null ? new [] { employee.BusinessUnit.Id } : Array.Empty<Guid>();

        // Act
        var result =
            ExpressionEvaluateHelper.InlineEvaluate(ee =>
            {
                return singlePath.Select(IEnumerable<Guid> (securityContext) =>
                    securityContext != null ? new[] { ee.Evaluate(idPath, securityContext) } : Array.Empty<Guid>());
            });


        // Assert
        result.Should().Be(expectedResult, ExpressionComparer.Value);
    }
}