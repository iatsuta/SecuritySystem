using ExampleApp.Application;
using ExampleApp.Domain;

using GenericQueryable;

using Microsoft.AspNetCore.Mvc;

using SecuritySystem;
using SecuritySystem.UserSource;

namespace ExampleApp.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController(
    ICurrentUserSource<Employee> currentUserSource,
    IRepositoryFactory<TestObject> testObjectRepositoryFactory,
    IRepositoryFactory<BusinessUnit> buRepositoryFactory,
    IRepositoryFactory<Employee> employeeRepositoryFactory) : ControllerBase
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

    [HttpGet]
    public async Task<string> GetCurrentUserLoginByEmployee(CancellationToken cancellationToken = default)
    {
        return await employeeRepositoryFactory.Create(SecurityRule.View)
            .GetQueryable()
            .Select(employee => employee.Login)
            .GenericSingleAsync(cancellationToken);
    }

    [HttpGet]
    public async Task<IEnumerable<BuDto>> GetBuList(CancellationToken cancellationToken = default)
    {
        return await buRepositoryFactory
            .Create(SecurityRule.View)
            .GetQueryable()
            .Select(bu => new BuDto(bu.Id, bu.Name, bu.Parent == null ? null : bu.Parent.Id))
            .GenericToListAsync(cancellationToken);
    }
}