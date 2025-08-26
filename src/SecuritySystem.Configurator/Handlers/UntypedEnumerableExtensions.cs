namespace SecuritySystem.Configurator.Handlers;

public static class UntypedEnumerableExtensions
{
    public static Array ToArray(this System.Collections.IEnumerable source, Type elementType)
    {
        var sourceArr = source.Cast<object>().ToArray();

        var array = Array.CreateInstance(elementType, sourceArr.Length);

        sourceArr.CopyTo(array, 0);

        return array;
    }
}