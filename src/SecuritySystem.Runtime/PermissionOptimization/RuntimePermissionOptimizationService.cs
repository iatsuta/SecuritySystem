namespace SecuritySystem.PermissionOptimization;

public class RuntimePermissionOptimizationService : IRuntimePermissionOptimizationService
{
    public IEnumerable<Dictionary<Type, Array>> Optimize(IEnumerable<Dictionary<Type, Array>> permissions)
    {
        var cachedPermissions = permissions.ToList();

        var orderedTypes = cachedPermissions
            .SelectMany(p => p)
            .GroupBy(p => p.Key)
            .OrderByDescending(g => g.SelectMany(p => p.Value.Cast<object>()).Distinct().Count())
            .Select(g => g.Key)
            .ToList();

        if (orderedTypes.Count == 0)
            return cachedPermissions;

        IEnumerable<Dictionary<Type, HashSet<object>>>? current = null;

        foreach (var type in orderedTypes)
        {
            var grouped = GetGroupable(current, cachedPermissions, type)
                .GroupBy(item => item.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(i => i.Value ?? Enumerable.Empty<object>())
                          .ToHashSet()
                );

            if (grouped.TryGetValue(GroupKey.Empty, out var baseSet))
            {
                if (baseSet.Count == 0)
                    return [new Dictionary<Type, Array>()];

                grouped.Remove(GroupKey.Empty);

                var toRemove = RefineGroupedPermissions(grouped, baseSet);
                foreach (var k in toRemove)
                    grouped.Remove(k);

                grouped[GroupKey.Empty] = baseSet;
            }

            current = grouped.Select(g =>
                g.Key.GetKeyPairs()
                     .Concat(g.Value.Count > 0 ? [KeyValuePair.Create(type, g.Value)] : Array.Empty<KeyValuePair<Type, HashSet<object>>>())
                     .ToDictionary(p => p.Key, p => p.Value)
            );
        }

        return current?.Select(d => d.ToDictionary(
            p => p.Key,
            p =>
            {
                var elementType = GetElementType(d, p.Key);
                var arr = Array.CreateInstance(elementType, p.Value.Count);
                p.Value.ToArray().CopyTo(arr, 0);
                return arr;
            }))
            ?? [];
    }


    private static IEnumerable<GroupableItem> GetGroupable(
        IEnumerable<Dictionary<Type, HashSet<object>>>? main,
        IEnumerable<Dictionary<Type, Array>> additional,
        Type currentType) =>
        main?.Select(p => new GroupableItem(new GroupKey(p, currentType), p.ContainsKey(currentType) ? p[currentType] : null))
        ?? additional.Select(p => new GroupableItem(new GroupKey(p, currentType), p.ContainsKey(currentType) ? p[currentType].Cast<object>().ToHashSet() : null));

    private static List<GroupKey> RefineGroupedPermissions(
        Dictionary<GroupKey, HashSet<object>> grouped,
        HashSet<object> removeItems)
    {
        var removed = new List<GroupKey>();
        foreach (var pair in grouped)
        {
            if (pair.Value.Count == 0)
                continue;

            pair.Value.ExceptWith(removeItems);
            if (pair.Value.Count == 0)
                removed.Add(pair.Key);
        }
        return removed;
    }

    private static Type GetElementType(Dictionary<Type, HashSet<object>> d, Type key)
    {
        if (d.TryGetValue(key, out var set) && set.Count > 0)
            return set.First()!.GetType();
        return typeof(object);
    }

    private record GroupableItem(GroupKey Key, HashSet<object>? Value);

    private sealed class GroupKey : IEquatable<GroupKey>
    {
        private readonly int hashCode;
        private readonly IDictionary<Type, HashSet<object>> keyData;

        public static readonly GroupKey Empty = new(new Dictionary<Type, Array>(), typeof(GroupKey));

        public GroupKey(Dictionary<Type, Array> dataItem, Type excludedType)
        {
            keyData = new Dictionary<Type, HashSet<object>>(dataItem.Count);
            foreach (var pair in dataItem)
            {
                if (pair.Key == excludedType)
                    continue;

                var set = new HashSet<object>();
                foreach (var el in pair.Value)
                    set.Add(el!);

                keyData.Add(pair.Key, set);
            }
            hashCode = CalculateHashCode();
        }

        public GroupKey(Dictionary<Type, HashSet<object>> dataItem, Type excludedType)
        {
            keyData = new Dictionary<Type, HashSet<object>>(dataItem.Where(pair => pair.Key != excludedType));
            hashCode = CalculateHashCode();
        }

        public IEnumerable<KeyValuePair<Type, HashSet<object>>> GetKeyPairs() => keyData;
        public override int GetHashCode() => hashCode;

        public bool Equals(GroupKey? other) => Equals((object?)other);

        public override bool Equals(object? obj) =>
            obj is GroupKey gk && DataEquals(gk);

        private int CalculateHashCode()
        {
            var result = 0;
            foreach (var pair in keyData)
            {
                result ^= pair.Key.GetHashCode();
                foreach (var val in pair.Value)
                {
                    result ^= val.GetHashCode();
                }
            }
            return result;
        }

        private bool DataEquals(GroupKey other)
        {
            if (keyData.Count != other.keyData.Count)
                return false;

            foreach (var pair in keyData)
            {
                if (!other.keyData.TryGetValue(pair.Key, out var otherSet))
                    return false;

                if (!pair.Value.SetEquals(otherSet))
                    return false;
            }
            return true;
        }
    }
}