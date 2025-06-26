using ExampleWebApp.Application;
using ExampleWebApp.Domain;

using GenericQueryable;

using Microsoft.AspNetCore.Mvc;

using SecuritySystem;
using SecuritySystem.UserSource;

namespace ExampleWebApp.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController(
    ICurrentUserSource<Employee> currentUserSource,
    IRepositoryFactory<TestObject> testObjectRepositoryFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<TestObjectDto>> GetTestObjects(CancellationToken cancellationToken = default)
    {
        return await testObjectRepositoryFactory
            .Create(SecurityRule.View)
            .GetQueryable()
            .Select(testObj => new TestObjectDto(testObj.Id, testObj.BusinessUnit.Name))
            .GenericToListAsync(cancellationToken);
    }

    [HttpGet]
    public async Task<string> GetCurrentUserLogin(CancellationToken cancellationToken = default)
    {
        return currentUserSource.CurrentUser.Login;
    }
}

public record TestObjectDto(Guid Id, string BuName);