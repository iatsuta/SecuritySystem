namespace SecuritySystem;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<T?> task)
    {
        var value = await task;

        if (value is not null)
        {
            yield return value;
        }
    }

    public static IAsyncEnumerable<T> GetAllElements<T>(this IAsyncEnumerable<T> source, Func<T, IAsyncEnumerable<T>> getChildFunc)
    {
        return source.SelectMany(child => child.GetAllElements(getChildFunc));
    }

    public static IAsyncEnumerable<T> GetAllElements<T>(this T? source, Func<T, Task<T?>> getNextFunc, bool skipFirstElement)
        where T : class
    {
        var baseElements = source.GetAllElements(getNextFunc);

        return skipFirstElement ? baseElements.Skip(1) : baseElements;
    }

    public static async IAsyncEnumerable<T> GetAllElements<T>(this T? source, Func<T, Task<T?>> getNextFunc)
        where T : class
    {
        for (var state = source; state != null; state = await getNextFunc(state))
        {
            yield return state;
        }
    }

    public static async IAsyncEnumerable<T> GetAllElements<T>(this T source, Func<T, IAsyncEnumerable<T>> getChildFunc)
    {
        yield return source;

        await foreach (var element in getChildFunc(source).SelectMany(child => child.GetAllElements(getChildFunc)))
        {
            yield return element;
        }
    }

    extension<TSource>(IAsyncEnumerable<TSource> source)
    {
        public IAsyncEnumerable<TResult> SelectAsync<TResult>(Func<TSource, Task<TResult>> selector) => new SelectAsyncAsyncEnumerable<TSource, TResult>(source, (s, _) => selector(s));

        public IAsyncEnumerable<TResult> SelectAsync<TResult>(Func<TSource, CancellationToken, Task<TResult>> selector) => new SelectAsyncAsyncEnumerable<TSource, TResult>(source, selector);
    }

    private class SelectAsyncAsyncEnumerable<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector) : IAsyncEnumerable<TResult>
    {
        public async IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                yield return await selector(item, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}