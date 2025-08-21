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

        var aggregatePermissions = groupedPermissions.Select(pair => new Dictionary<Type, Array>
        {
            { pair.Key, pair.SelectMany(g => g.Values.AsEnumerable().Single()).Distinct().ToArray() }
        });

        var withoutAggregatePermissions = cachedPermissions.Except(groupedPermissions.SelectMany(g => g));

        return aggregatePermissions.Concat(withoutAggregatePermissions);
    }
}