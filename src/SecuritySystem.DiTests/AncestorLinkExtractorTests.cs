using SecuritySystem.AncestorDenormalization;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.DiTests;

public class AncestorLinkExtractorTests
{
    public class DomainObject
    {
        public required string Name { get; init; }

        public DomainObject? Parent { get; set; }

        public override string ToString() => this.Name;
    }

    public record DirectAncestorLink
    {
        public required DomainObject From { get; init; }

        public required DomainObject To { get; init; }

        public override string ToString()
        {
            return $"{nameof(this.From)}:{this.From}|{nameof(this.To)}:{this.To}";
        }
	}
    public record UnDirectAncestorLink
    {
	    public required DomainObject From { get; init; }

	    public required DomainObject To { get; init; }

	    public override string ToString()
	    {
		    return $"{nameof(this.From)}:{this.From}|{nameof(this.To)}:{this.To}";
	    }
    }


	[Theory]
    [MemberData(nameof(GetMoveCases))]
    public async Task MoveNode_UpdatesLinksCorrectly(MoveTestCase testCase)
    {
        // Arrange
        var queryableSource = Substitute.For<IQueryableSource>();
        var ct = CancellationToken.None;

        queryableSource.GetQueryable<DomainObject>().Returns(testCase.Nodes.AsQueryable());
        queryableSource.GetQueryable<DirectAncestorLink>().Returns(testCase.ExistingOldLinks.AsQueryable());

        var extractor = new AncestorLinkExtractor<DomainObject, DirectAncestorLink>(
            queryableSource,
            new DomainObjectExpander<DomainObject>(new HierarchicalInfo<DomainObject>(v => v.Parent), queryableSource),
            new FullAncestorLinkInfo<DomainObject, DirectAncestorLink, UnDirectAncestorLink>(
                new AncestorLinkInfo<DomainObject, DirectAncestorLink>(l => l.From, l => l.To),
                new AncestorLinkInfo<DomainObject, UnDirectAncestorLink>(l => l.From, l => l.To)));

        // Act
        foreach (var pair in testCase.UpdateParents)
        {
            pair.Key.Parent = pair.Value;
        }

        var result = await extractor.GetSyncResult(testCase.UpdateParents.Keys, [], ct);

        // Assert
        var orderedResult =
            new SyncResult<DomainObject, DirectAncestorLink>(
                result.Adding.OrderBy(link => link.Ancestor.Name).ThenBy(link => link.Child.Name),
                result.Removing.OrderBy(link => link.From.Name).ThenBy(link => link.To.Name));

        orderedResult.Should().Be(testCase.ExpectedResult);
    }

    public record MoveTestCase(
        IReadOnlyList<DomainObject> Nodes,
        IReadOnlyList<DirectAncestorLink> ExistingOldLinks,
        IReadOnlyDictionary<DomainObject, DomainObject> UpdateParents,
        SyncResult<DomainObject, DirectAncestorLink> ExpectedResult);

    public static IEnumerable<object[]> GetMoveCases()
    {
        return

            from testCase in new[] { GetMoveCase0(), GetMoveCase1(), GetMoveCase2() }

            select new[] { testCase };
    }

    public static MoveTestCase GetMoveCase0()
    {
        var a = new DomainObject { Name = "A" };
        var b = new DomainObject { Name = "B", Parent = a };
        var c = new DomainObject { Name = "C", Parent = b };

        var updateParents = new Dictionary<DomainObject, DomainObject>
        {
            { c, a }
        };

        var nodes = new[] { a, b, c };

        var existingOldLinks = new List<DirectAncestorLink>
        {
            new() { From = a, To = a },
            new() { From = a, To = b },
            new() { From = a, To = c },
            new() { From = b, To = b },
            new() { From = b, To = c },
            new() { From = c, To = c },
        };

        var expectedResult = new SyncResult<DomainObject, DirectAncestorLink>(
            [],
            [new DirectAncestorLink { From = b, To = c }]);

        return new MoveTestCase(nodes, existingOldLinks, updateParents, expectedResult);
    }

    public static MoveTestCase GetMoveCase1()
    {
        /* A
           ├─ A1
           │  ├─ B1 (→ A2)
           │  │  ├─ C1
           │  │  └─ C2
           │  └─ B2
           └─ A2 */

        var a = new DomainObject { Name = "A" };
        var a1 = new DomainObject { Name = "A1", Parent = a };
        var a2 = new DomainObject { Name = "A2", Parent = a };
        var b1 = new DomainObject { Name = "B1", Parent = a1 };
        var b2 = new DomainObject { Name = "B2", Parent = a1 };
        var c1 = new DomainObject { Name = "C1", Parent = b1 };
        var c2 = new DomainObject { Name = "C2", Parent = b1 };

        var nodes = new[] { a, a1, a2, b1, b2, c1, c2 };

        var existingOldLinks = new List<DirectAncestorLink>
        {
            new() { From = a, To = a },
            new() { From = a, To = a1 },
            new() { From = a, To = a2 },
            new() { From = a, To = b1 },
            new() { From = a, To = b2 },
            new() { From = a, To = c1 },
            new() { From = a, To = c2 },
            new() { From = a1, To = a1 },
            new() { From = a1, To = b1 },
            new() { From = a1, To = b2 },
            new() { From = a1, To = c1 },
            new() { From = a1, To = c2 },
            new() { From = b1, To = b1 },
            new() { From = b1, To = c1 },
            new() { From = c1, To = c1 },
            new() { From = b1, To = c2 },
            new() { From = c2, To = c2 },
        };

        var updateParents = new Dictionary<DomainObject, DomainObject>
        {
            { b1, a2 }
        };

        var expectedResult = new SyncResult<DomainObject, DirectAncestorLink>(
            new[]
            {
                new AncestorLinkInfo<DomainObject>(a2, b1),
                new AncestorLinkInfo<DomainObject>(a2, c1),
                new AncestorLinkInfo<DomainObject>(a2, c2)
            },
            new[]
            {
                new DirectAncestorLink { From = a1, To = b1 },
                new DirectAncestorLink { From = a1, To = c1 },
                new DirectAncestorLink { From = a1, To = c2 }
            });

        return new MoveTestCase(nodes, existingOldLinks, updateParents, expectedResult);
    }

    public static MoveTestCase GetMoveCase2()
    {
        var a = new DomainObject { Name = "A" };
        var a1 = new DomainObject { Name = "A1", Parent = a };
        var a1A = new DomainObject { Name = "A1A", Parent = a1 };
        var a1A1 = new DomainObject { Name = "A1A1", Parent = a1A };
        var a1B = new DomainObject { Name = "A1B", Parent = a1 };
        var a1B1 = new DomainObject { Name = "A1B1", Parent = a1B };
        var a1B1A = new DomainObject { Name = "A1B1A", Parent = a1B1 };
        var a1B1B = new DomainObject { Name = "A1B1B", Parent = a1B1 };
        var a1B1B1 = new DomainObject { Name = "A1B1B1", Parent = a1B1B };
        var a1B1C = new DomainObject { Name = "A1B1C", Parent = a1B1 };
        var a2 = new DomainObject { Name = "A2", Parent = a };
        var a2A = new DomainObject { Name = "A2A", Parent = a2 };
        var a2A1 = new DomainObject { Name = "A2A1", Parent = a2A };
        var a2B = new DomainObject { Name = "A2B", Parent = a2 };

        var updateParents = new Dictionary<DomainObject, DomainObject>
        {
            { a1B1, a2A }
        };

        var nodes = new[] { a, a1, a1A, a1A1, a1B, a1B1, a1B1A, a1B1B, a1B1B1, a1B1C, a2, a2A, a2A1, a2B };

        var existingOldLinks = new List<DirectAncestorLink>
        {
            new() { From = a, To = a },
            new() { From = a, To = a1 },
            new() { From = a, To = a1A },
            new() { From = a, To = a1A1 },
            new() { From = a, To = a1B },
            new() { From = a, To = a1B1 },
            new() { From = a, To = a1B1A },
            new() { From = a, To = a1B1B },
            new() { From = a, To = a1B1B1 },
            new() { From = a, To = a1B1C },
            new() { From = a, To = a2 },
            new() { From = a, To = a2A },
            new() { From = a, To = a2A1 },
            new() { From = a, To = a2B },

            new() { From = a1, To = a1 },
            new() { From = a1, To = a1A },
            new() { From = a1, To = a1A1 },
            new() { From = a1, To = a1B },
            new() { From = a1, To = a1B1 },
            new() { From = a1, To = a1B1A },
            new() { From = a1, To = a1B1B },
            new() { From = a1, To = a1B1B1 },
            new() { From = a1, To = a1B1C },

            new() { From = a1A, To = a1A },
            new() { From = a1A, To = a1A1 },

            new() { From = a1A1, To = a1A1 },

            new() { From = a1B, To = a1B },
            new() { From = a1B, To = a1B1 },
            new() { From = a1B, To = a1B1A },
            new() { From = a1B, To = a1B1B },
            new() { From = a1B, To = a1B1B1 },
            new() { From = a1B, To = a1B1C },

            new() { From = a1B1, To = a1B1 },
            new() { From = a1B1, To = a1B1A },
            new() { From = a1B1, To = a1B1B },
            new() { From = a1B1, To = a1B1B1 },
            new() { From = a1B1, To = a1B1C },

            new() { From = a1B1A, To = a1B1A },
            new() { From = a1B1B, To = a1B1B },
            new() { From = a1B1B, To = a1B1B1 },
            new() { From = a1B1B1, To = a1B1B1 },
            new() { From = a1B1C, To = a1B1C },

            new() { From = a2, To = a2 },
            new() { From = a2, To = a2A },
            new() { From = a2, To = a2A1 },
            new() { From = a2, To = a2B },

            new() { From = a2A, To = a2A },
            new() { From = a2A, To = a2A1 },

            new() { From = a2A1, To = a2A1 },

            new() { From = a2B, To = a2B }
        };

        var expectedResult = new SyncResult<DomainObject, DirectAncestorLink>(
            new[]
            {
                new AncestorLinkInfo<DomainObject>(a2, a1B1),
                new AncestorLinkInfo<DomainObject>(a2, a1B1A),
                new AncestorLinkInfo<DomainObject>(a2, a1B1B),
                new AncestorLinkInfo<DomainObject>(a2, a1B1B1),
                new AncestorLinkInfo<DomainObject>(a2, a1B1C),
                new AncestorLinkInfo<DomainObject>(a2A, a1B1),
                new AncestorLinkInfo<DomainObject>(a2A, a1B1A),
                new AncestorLinkInfo<DomainObject>(a2A, a1B1B),
                new AncestorLinkInfo<DomainObject>(a2A, a1B1B1),
                new AncestorLinkInfo<DomainObject>(a2A, a1B1C)
            },
            new[]
            {
                new DirectAncestorLink { From = a1, To = a1B1 },
                new DirectAncestorLink { From = a1, To = a1B1A },
                new DirectAncestorLink { From = a1, To = a1B1B },
                new DirectAncestorLink { From = a1, To = a1B1B1 },
                new DirectAncestorLink { From = a1, To = a1B1C },
                new DirectAncestorLink { From = a1B, To = a1B1 },
                new DirectAncestorLink { From = a1B, To = a1B1A },
                new DirectAncestorLink { From = a1B, To = a1B1B },
                new DirectAncestorLink { From = a1B, To = a1B1B1 },
                new DirectAncestorLink { From = a1B, To = a1B1C }
            });

        return new MoveTestCase(nodes, existingOldLinks, updateParents, expectedResult);
    }
}