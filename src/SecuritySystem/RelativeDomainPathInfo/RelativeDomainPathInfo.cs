using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.RelativeDomainPathInfo;

public record SingleRelativeDomainPathInfo<TFrom, TTo>(Expression<Func<TFrom, TTo>> Path) : IRelativeDomainPathInfo<TFrom, TTo>
{
    private readonly Func<TFrom, TTo> pathFunc = Path.Compile();


    public IRelativeDomainPathInfo<TNewFrom, TTo> OverrideInput<TNewFrom>(Expression<Func<TNewFrom, TFrom>> selector)
    {
        return new SingleRelativeDomainPathInfo<TNewFrom, TTo>(this.Path.OverrideInput(selector));
    }

    public IRelativeDomainPathInfo<TFrom, TNewTo> Select<TNewTo>(Expression<Func<TTo, TNewTo>> selector)
    {
        return new SingleRelativeDomainPathInfo<TFrom, TNewTo>(this.Path.Select(selector));
    }

    public IRelativeDomainPathInfo<TFrom, TNewTo> Select<TNewTo>(Expression<Func<TTo, IEnumerable<TNewTo>>> selector)
    {
        return new ManyRelativeDomainPathInfo<TFrom, TNewTo>(this.Path.Select(selector));
    }

    public Expression<Func<TFrom, bool>> CreateCondition(Expression<Func<TTo, bool>> filter)
    {
        return this.Path.Select(filter);
    }

    public IEnumerable<TTo> GetRelativeObjects(TFrom source)
    {
        var relativeObject = this.pathFunc(source);

        if (relativeObject != null)
        {
            yield return relativeObject;
        }
    }
}