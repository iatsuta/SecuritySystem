namespace SecuritySystem.PermissionOptimization;

public interface IRuntimePermissionOptimizationService
{
    IEnumerable<Dictionary<Type, Array>> Optimize(IEnumerable<Dictionary<Type, Array>> permissions);
}
