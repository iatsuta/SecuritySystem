using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;

namespace SecuritySystem.RelativeDomainPathInfo;

public record ManyRelativeDomainPathInfo<TFrom, TTo>(Expression<Func<TFrom, IEnumerable<TTo>>> Path) : IRelativeDomainPathInfo<TFrom, TTo>
{
    private readonly Func<TFrom, IEnumerable<TTo>> pathFunc = Path.Compile();

    public IRelativeDomainPathInfo<TNewFrom, TTo> OverrideInput<TNewFrom>(Expression<Func<TNewFrom, TFrom>> selector)
    {
        return new ManyRelativeDomainPathInfo<TNewFrom, TTo>(this.Path.OverrideInput(selector));
    }

    public IRelativeDomainPathInfo<TFrom, TNewTo> Select<TNewTo>(Expression<Func<TTo, TNewTo>> selector)
    {
        return new ManyRelativeDomainPathInfo<TFrom, TNewTo>(

            ExpressionEvaluateHelper.InlineEvaluate(ee =>

                this.Path.Select(items => items.Select(item => ee.Evaluate(selector, item)))));
    }

    public IRelativeDomainPathInfo<TFrom, TNewTo> Select<TNewTo>(Expression<Func<TTo, IEnumerable<TNewTo>>> selector)
    {
        return new ManyRelativeDomainPathInfo<TFrom, TNewTo>(

            ExpressionEvaluateHelper.InlineEvaluate(ee =>

                this.Path.Select(items => items.SelectMany(item => ee.Evaluate(selector, item)))));
    }

    public Expression<Func<TFrom, bool>> CreateCondition(Expression<Func<TTo, bool>> filter)
    {
        return ExpressionEvaluateHelper.InlineEvaluate(ee =>

            this.Path.Select(items => items.Any(item => ee.Evaluate(filter, item))));
    }

    public IEnumerable<TTo> GetRelativeObjects(TFrom source)
    {
        return this.pathFunc(source);
    }
}