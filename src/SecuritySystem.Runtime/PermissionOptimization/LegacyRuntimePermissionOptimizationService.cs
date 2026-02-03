namespace SecuritySystem.PermissionOptimization;

public class LegacyRuntimePermissionOptimizationService : IRuntimePermissionOptimizationService
{
    public IEnumerable<Dictionary<Type, Array>> Optimize(IEnumerable<Dictionary<Type, Array>> permissions)
    {
        var cachedPermissions = permissions.ToList();

        var groupedPermissionsRequest =

            from permission in cachedPermissions

            where permission.Count == 1

            let pair = permission.Single()

            group permission by pair.Key;

        var groupedPermissions = groupedPermissionsRequest.ToList();

        var aggregatePermissions = groupedPermissions.Select(pair =>
        {
            var arrays = pair.Select(p => p.Values.Single()).ToList();

            var elementType = arrays.First().GetType().GetElementType()
                              ?? throw new InvalidOperationException("Unknown array element type");

            var combined = arrays
                .SelectMany(arr => arr.Cast<object>())
                .Distinct()
                .ToArray();

            var typedArray = Array.CreateInstance(elementType, combined.Length);
            combined.CopyTo(typedArray, 0);

            return new Dictionary<Type, Array>
            {
                { pair.Key, typedArray }
            };
        });

        var withoutAggregatePermissions = cachedPermissions
            .Except(groupedPermissions.SelectMany(g => g));

        return aggregatePermissions.Concat(withoutAggregatePermissions);
    }
}