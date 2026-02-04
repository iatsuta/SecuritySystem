using ExampleApp.Domain;
using ExampleApp.Domain.Auth.Virtual;
using ExampleApp.Infrastructure;

using HierarchicalExpand.AncestorDenormalization;

using Microsoft.AspNetCore.Mvc;

using SecuritySystem.GeneralPermission.Initialize;
using SecuritySystem.Services;

namespace ExampleApp.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class InitController(
	IRawUserAuthenticationService rawUserAuthenticationService,
	IDenormalizedAncestorsService<BusinessUnit> denormalizedAncestorsService,
	ISecurityRoleInitializer securityRoleInitializer,
    ISecurityContextInitializer securityContextInitializer,
    TestDbContext dbContext) : ControllerBase
{
	[HttpPost]
	public async Task TestInitialize(CancellationToken cancellationToken = default)
	{
		await dbContext.Database.EnsureDeletedAsync(cancellationToken);
		await dbContext.Database.EnsureCreatedAsync(cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);

		await dbContext.EnsureViewsCreatedAsync(cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);

		{
			var currentEmployee = new Employee { Login = rawUserAuthenticationService.GetUserName() };
			dbContext.Add(currentEmployee);

			var currentEmployeeAdminVirtualPermission = new Administrator { Employee = currentEmployee };
			dbContext.Add(currentEmployeeAdminVirtualPermission);
		}

		var testRootBu = new BusinessUnit() { Name = "TestRootBu" };
		dbContext.Add(testRootBu);

		foreach (var index in Enumerable.Range(1, 2))
		{
			var testLocation = new Location { Name = $"Test{nameof(Location)}{index}" };

			var testBu = new BusinessUnit { Name = $"Test{nameof(BusinessUnit)}{index}", Parent = testRootBu };
			dbContext.Add(testBu);

			var testChildBu = new BusinessUnit { Name = $"Test{nameof(BusinessUnit)}{index}-Child", Parent = testBu };
			dbContext.Add(testChildBu);

            var testObj = new TestObject { BusinessUnit = testChildBu, Location = testLocation };
            dbContext.Add(testObj);

            var testEmployee = new Employee { Login = $"Test{nameof(Employee)}{index}" };
			dbContext.Add(testEmployee);

			var testVirtualPermission = new TestManager { BusinessUnit = testBu, Employee = testEmployee, Location = testLocation };
			dbContext.Add(testVirtualPermission);
		}

		await dbContext.SaveChangesAsync(cancellationToken);

		await denormalizedAncestorsService.SyncAllAsync(cancellationToken);

		await dbContext.SaveChangesAsync(cancellationToken);

        await securityRoleInitializer.Init(cancellationToken);
        await securityContextInitializer.Init(cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}