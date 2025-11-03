using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.DiTests;

public class DomainObjectExpanderTests
{
    public class DomainObject
    {
        public required string Name { get; init; }

        public DomainObject? Parent { get; init; }

        public override string ToString() => this.Name;
    }

    /*
        Tree (depth = 6, branched):

        A
        ├─ A1
        │  ├─ A1a
        │  │  ├─ A1a1
        │  │  │  └─ A1a1x
        │  │  └─ A1a2
        │  └─ A1b
        │     └─ A1b1
        │        └─ A1b1x
        └─ A2
           └─ A2a

        B
        ├─ B1
        │  ├─ B1a
        │  │  └─ B1a1
        │  └─ B1b
        │     ├─ B1b1
        │     │  └─ B1b1x
        │     └─ B1b2
        └─ B2

        C
        ├─ C1
        │  ├─ C1a
        │  │  └─ C1a1
        │  └─ C1b
        │     └─ C1b1
        └─ C2
    */

    private static IEnumerable<DomainObject> BuildTree()
    {
        var a = new DomainObject { Name = "A" };
        var a1 = new DomainObject { Name = "A1", Parent = a };
        var a1A = new DomainObject { Name = "A1a", Parent = a1 };
        var a1A1 = new DomainObject { Name = "A1a1", Parent = a1A };
        var a1A1X = new DomainObject { Name = "A1a1x", Parent = a1A1 };
        var a1A2 = new DomainObject { Name = "A1a2", Parent = a1A };
        var a1B = new DomainObject { Name = "A1b", Parent = a1 };
        var a1B1 = new DomainObject { Name = "A1b1", Parent = a1B };
        var a1B1X = new DomainObject { Name = "A1b1x", Parent = a1B1 };
        var a2 = new DomainObject { Name = "A2", Parent = a };
        var a2A = new DomainObject { Name = "A2a", Parent = a2 };

        var b = new DomainObject { Name = "B" };
        var b1 = new DomainObject { Name = "B1", Parent = b };
        var b1A = new DomainObject { Name = "B1a", Parent = b1 };
        var b1A1 = new DomainObject { Name = "B1a1", Parent = b1A };
        var b1B = new DomainObject { Name = "B1b", Parent = b1 };
        var b1B1 = new DomainObject { Name = "B1b1", Parent = b1B };
        var b1B1X = new DomainObject { Name = "B1b1x", Parent = b1B1 };
        var b1B2 = new DomainObject { Name = "B1b2", Parent = b1B };
        var b2 = new DomainObject { Name = "B2", Parent = b };

        var c = new DomainObject { Name = "C" };
        var c1 = new DomainObject { Name = "C1", Parent = c };
        var c1A = new DomainObject { Name = "C1a", Parent = c1 };
        var c1A1 = new DomainObject { Name = "C1a1", Parent = c1A };
        var c1B = new DomainObject { Name = "C1b", Parent = c1 };
        var c1B1 = new DomainObject { Name = "C1b1", Parent = c1B };
        var c2 = new DomainObject { Name = "C2", Parent = c };

        return
        [
            a, a1, a1A, a1A1, a1A1X, a1A2, a1B, a1B1, a1B1X, a2, a2A,
            b, b1, b1A, b1A1, b1B, b1B1, b1B1X, b1B2, b2,
            c, c1, c1A, c1A1, c1B, c1B1, c2
        ];
    }

    private static readonly IReadOnlyList<DomainObject> AllNodes = BuildTree().ToArray();

    public static IEnumerable<object[]> GetTraversalCases()
    {
        yield return
        [
            new[] { AllNodes.Single(x => x.Name == "A1a1x") }, // startNodes
            AllNodes.Where(x => new[] { "A1a1x", "A1a1", "A1a", "A1", "A" }.Contains(x.Name)).ToArray(), // expectedResult
            true, // expandUp
            5 // expectedQueryCount
        ];

        yield return
        [
            new[] { AllNodes.Single(x => x.Name == "A1") },
            AllNodes.Where(x => new[] { "A1", "A1a", "A1a1", "A1a1x", "A1a2", "A1b", "A1b1", "A1b1x" }.Contains(x.Name)).ToArray(),
            false, // expandDown
            4 // expectedQueryCount (по числу слоёв)
        ];

        yield return
        [
            new[] { AllNodes.Single(x => x.Name == "B1b1x") },
            AllNodes.Where(x => new[] { "B1b1x", "B1b1", "B1b", "B1", "B" }.Contains(x.Name)).ToArray(),
            true,
            5
        ];

        yield return
        [
            new[] { AllNodes.Single(x => x.Name == "B1") },
            AllNodes.Where(x => new[] { "B1", "B1a", "B1a1", "B1b", "B1b1", "B1b1x", "B1b2" }.Contains(x.Name)).ToArray(),
            false,
            4
        ];

        yield return
        [
            new[] { AllNodes.Single(x => x.Name == "C1a1") },
            AllNodes.Where(x => new[] { "C1a1", "C1a", "C1", "C" }.Contains(x.Name)).ToArray(),
            true,
            4
        ];

        yield return
        [
            new[] { AllNodes.Single(x => x.Name == "C1") },
            AllNodes.Where(x => new[] { "C1", "C1a", "C1a1", "C1b", "C1b1" }.Contains(x.Name)).ToArray(),
            false,
            3
        ];
    }

    [Theory]
    [MemberData(nameof(GetTraversalCases))]
    public async Task HierarchyTraversal_ShouldReturnExpected(
        IEnumerable<DomainObject> startNodes,
        IEnumerable<DomainObject> expectedResult,
        bool expandUp,
        int expectedQueryCount)
    {
        // Arrange
        var ct = CancellationToken.None;

        var queryableSource = Substitute.For<IQueryableSource>();
        queryableSource.GetQueryable<DomainObject>().Returns(_ => AllNodes.AsQueryable());

        var expander = new DomainObjectExpander<DomainObject>(
            new HierarchicalInfo<DomainObject>(x => x.Parent),
            queryableSource);

        // Act
        var result = expandUp
            ? await expander.GetAllParents(startNodes, ct)
            : await expander.GetAllChildren(startNodes, ct);

        // Assert
        result.OrderBy(v => v.Name).Should().BeEquivalentTo(expectedResult.OrderBy(v => v.Name));
        queryableSource.Received(expectedQueryCount).GetQueryable<DomainObject>();
    }
}