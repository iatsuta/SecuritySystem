using CommonFramework;

using ExampleWebApp.Domain;
using ExampleWebApp.Infrastructure;

using GenericQueryable;

using Microsoft.AspNetCore.Mvc;

using SecuritySystem;
using SecuritySystem.DomainServices;
using SecuritySystem.UserSource;

namespace ExampleWebApp.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController(
    ICurrentUserSource<Employee> currentUserSource,
    TestDbContext dbContext,
    IDomainSecurityService<TestObject> domainSecurityService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<TestObjectDto>> GetTestObjects(CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<TestObject>()
            .Pipe(domainSecurityService.GetSecurityProvider(SecurityRule.View).InjectFilter)
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