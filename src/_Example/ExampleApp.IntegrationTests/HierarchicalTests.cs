using CommonFramework.GenericRepository;

using ExampleApp.Domain;

using GenericQueryable;

using HierarchicalExpand;

using Microsoft.Extensions.DependencyInjection;

namespace ExampleApp.IntegrationTests;

public class HierarchicalTests : TestBase
{
    [Fact]
    public async Task InvokeExpandWithParents_ForRootBu_DataCorrected()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var queryableSource = scope.ServiceProvider.GetRequiredService<IQueryableSource>();
        var hierarchicalObjectExpanderFactory = scope.ServiceProvider.GetRequiredService<IHierarchicalObjectExpanderFactory>();
        var hierarchicalObjectExpander = hierarchicalObjectExpanderFactory.Create<Guid>(typeof(BusinessUnit));

        var rootBuId = await queryableSource.GetQueryable<BusinessUnit>().Where(bu => bu.Parent == null).Select(bu => bu.Id)
            .GenericSingleAsync(cancellationToken);

        // Act
        var dict = hierarchicalObjectExpander.ExpandWithParents([rootBuId], HierarchicalExpandType.Parents);

        // Assert
        dict.Count.Should().Be(1);
        dict.Should().BeEquivalentTo(new Dictionary<Guid, Guid> { { rootBuId, Guid.Empty } });
    }
}