using ExampleWebApp.Domain;
using ExampleWebApp.Domain.Auth;
using ExampleWebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SecuritySystem.Services;

namespace ExampleWebApp.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class InitController(
    IRawUserAuthenticationService rawUserAuthenticationService,
    TestDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task TestInitialize(CancellationToken cancellationToken)
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
            var testBu = new BusinessUnit() { Name = $"TestBu{index}" };
            dbContext.Add(testBu);

            var testEmployee = new Employee { Login = $"testEmployee{index}" };
            dbContext.Add(testEmployee);
            
            var testObj = new TestObject { BusinessUnit = testBu };
            dbContext.Add(testObj);

            var testPermission = new TestManager { BusinessUnit = testBu, Employee = testEmployee };
            dbContext.Add(testPermission);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}