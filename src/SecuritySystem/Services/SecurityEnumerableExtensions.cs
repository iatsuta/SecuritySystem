using CommonFramework;

namespace SecuritySystem.Services;

public static class SecurityEnumerableExtensions
{
    public static MergeResult<TSource, TTarget> GetMergeResult<TSource, TTarget, TKey>(
        this IEnumerable<TSource> source,
        IEnumerable<TTarget> target,
        Func<TSource, TKey> sourceKeySelector,
        Func<TTarget, TKey> targetKeySelector,
        Func<TKey, TKey, bool> equalsFunc)
        where TKey : notnull
    {
        return source.GetMergeResult(target, sourceKeySelector, targetKeySelector, new EqualityComparerImpl<TKey>(equalsFunc, _ => 0));
    }

    public static MergeResult<TSource, TTarget> GetMergeResult<TSource, TTarget, TKey>(
        this IEnumerable<TSource> source,
        IEnumerable<TTarget> target,
        Func<TSource, TKey> sourceKeySelector,
        Func<TTarget, TKey> targetKeySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        var targetMap = target.ToDictionary(targetKeySelector, z => z, comparer ?? EqualityComparer<TKey>.Default);

        var removingItems = new List<TSource>();

        var combineItems = new List<ValueTuple<TSource, TTarget>>();

        foreach (var sourceItem in source)
        {
            var sourceKey = sourceKeySelector(sourceItem);

            if (targetMap.TryGetValue(sourceKey, out var targetItem))
            {
                combineItems.Add(ValueTuple.Create(sourceItem, targetItem));
                targetMap.Remove(sourceKey);
            }
            else
            {
                removingItems.Add(sourceItem);
            }
        }

        var addingItems = targetMap.Values.ToList();

        return new MergeResult<TSource, TTarget>(addingItems, combineItems, removingItems);
    }
}