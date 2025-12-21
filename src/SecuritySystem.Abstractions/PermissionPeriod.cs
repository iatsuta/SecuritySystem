using System.Linq.Expressions;

namespace SecuritySystem;

public record PermissionPeriod(DateTime? StartDate, DateTime? EndDate)
{
    public static Expression<Func<PermissionPeriod, bool>> GetContainsExpression(DateTime today)
    {
        return period => (period.StartDate == null || period.StartDate <= today) && (period.EndDate == null || today <= period.EndDate);
    }

    public bool IsIntersected(PermissionPeriod otherPeriod)
    {
        var start1 = this.StartDate ?? DateTime.MinValue;
        var end1 = this.EndDate ?? DateTime.MaxValue;

        var start2 = otherPeriod.StartDate ?? DateTime.MinValue;
        var end2 = otherPeriod.EndDate ?? DateTime.MaxValue;

        return start1 <= end2 && start2 <= end1;
    }

    public static PermissionPeriod Eternity { get; } = new (null, null);
}