using ExampleApp.Domain;
using ExampleApp.Domain.Auth;
using ExampleApp.Infrastructure;

using Microsoft.AspNetCore.Mvc;

using SecuritySystem.Services;

namespace ExampleApp.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class InitController(
    IRawUserAuthenticationService rawUserAuthenticationService,
    TestDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task TestInitialize(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        {
            var currentEmployee = new Employee { Login = rawUserAuthenticationService.GetUserName() };
            dbContext.Add(currentEmployee);

            var currentEmployeePermission = new Administrator { Employee = currentEmployee };
            dbContext.Add(currentEmployeePermission);
        }

        foreach (var index in Enumerable.Range(1, 2))
        {
            var testLocation = new Location { Name = $"Test{nameof(Location)}{index}" };

            var testBu = new BusinessUnit() { Name = $"Test{nameof(BusinessUnit)}{index}" };
            dbContext.Add(testBu);

            var testEmployee = new Employee { Login = $"Test{nameof(Employee)}{index}" };
            dbContext.Add(testEmployee);

            var testObj = new TestObject { BusinessUnit = testBu, Location = testLocation };
            dbContext.Add(testObj);

            var testPermission = new TestManager { BusinessUnit = testBu, Employee = testEmployee, Location = testLocation };
            dbContext.Add(testPermission);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}