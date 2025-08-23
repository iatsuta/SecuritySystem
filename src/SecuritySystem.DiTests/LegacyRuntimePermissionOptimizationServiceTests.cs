using SecuritySystem.PermissionOptimization;

namespace SecuritySystem.DiTests;

public class LegacyRuntimePermissionOptimizationServiceTests
{
    private readonly LegacyRuntimePermissionOptimizationService service = new();

    [Fact]
    public void Optimize_SingleTypeMultipleArrays_MergesAndDistincts()
    {
        var permissions = new List<Dictionary<Type, Array>>
        {
            new() { { typeof(string), new Guid[] { Guid.Parse("11111111-1111-1111-1111-111111111111") } } },
            new() { { typeof(string), new Guid[] { Guid.Parse("22222222-2222-2222-2222-222222222222") } } },
            new() { { typeof(string), new Guid[] { Guid.Parse("11111111-1111-1111-1111-111111111111") } } }
        };

        var result = service.Optimize(permissions).ToList();

        result.Should().HaveCount(1);
        var array = (Guid[])result[0][typeof(string)];
        array.Should().BeEquivalentTo(new[]
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222")
        });
    }

    [Fact]
    public void Optimize_DifferentTypes_DoNotMerge()
    {
        var permissions = new List<Dictionary<Type, Array>>
        {
            new() { { typeof(string), new Guid[] { Guid.NewGuid() } } },
            new() { { typeof(int), new Guid[] { Guid.NewGuid() } } }
        };

        var result = service.Optimize(permissions).ToList();

        result.Should().HaveCount(2);
        result.Any(d => d.ContainsKey(typeof(string))).Should().BeTrue();
        result.Any(d => d.ContainsKey(typeof(int))).Should().BeTrue();
    }

    [Fact]
    public void Optimize_MixedDictionaries_LeavesComplexUntouched()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        var permissions = new List<Dictionary<Type, Array>>
        {
            new() { { typeof(string), new Guid[] { guid1 } } },
            new() { { typeof(string), new Guid[] { guid2 } } },
            new()
            {
                { typeof(string), new Guid[] { guid1 } },
                { typeof(int), new Guid[] { guid2 } }
            }
        };

        var result = service.Optimize(permissions).ToList();

        result.Should().HaveCount(2);
        result.Any(d => d.Keys.Count == 1 && d.ContainsKey(typeof(string))).Should().BeTrue();
        result.Any(d => d.Keys.Count == 2).Should().BeTrue();
    }

    [Fact]
    public void Optimize_NoPermissions_ReturnsEmpty()
    {
        var result = service.Optimize(new List<Dictionary<Type, Array>>()).ToList();
        result.Should().BeEmpty();
    }
}