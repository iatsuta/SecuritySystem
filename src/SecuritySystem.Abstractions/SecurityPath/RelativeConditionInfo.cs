using System.Linq.Expressions;

using CommonFramework.ExpressionComparers;

// ReSharper disable once CheckNamespace
namespace SecuritySystem;

public abstract record RelativeConditionInfo
{
    public abstract Type RelativeDomainObjectType { get; }
}

public record RelativeConditionInfo<TRelativeDomainObject>(Expression<Func<TRelativeDomainObject, bool>> Condition)
    : RelativeConditionInfo
{
    public override Type RelativeDomainObjectType { get; } = typeof(TRelativeDomainObject);

    public virtual bool Equals(RelativeConditionInfo<TRelativeDomainObject>? other) =>
        ReferenceEquals(this, other)
        || (other is not null && ExpressionComparer.Default.Equals(this.Condition, other.Condition));

    public override int GetHashCode() => 0;
}