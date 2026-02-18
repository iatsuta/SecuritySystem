namespace SecuritySystem;

public record struct PermissionPeriod(DateTime? StartDate, DateTime? EndDate)
{
    public bool IsIntersected(PermissionPeriod otherPeriod)
    {
        var start1 = this.StartDate ?? DateTime.MinValue;
        var end1 = this.EndDate ?? DateTime.MaxValue;

        var start2 = otherPeriod.StartDate ?? DateTime.MinValue;
        var end2 = otherPeriod.EndDate ?? DateTime.MaxValue;

        return start1 <= end2 && start2 <= end1;
    }

    public bool Contains(PermissionPeriod otherPeriod)
    {
        var start1 = this.StartDate ?? DateTime.MinValue;
        var end1 = this.EndDate ?? DateTime.MaxValue;

        var start2 = otherPeriod.StartDate ?? DateTime.MinValue;
        var end2 = otherPeriod.EndDate ?? DateTime.MaxValue;

        return start1 <= start2 && end2 <= end1;
    }

    public static PermissionPeriod Eternity { get; } = new (null, null);
}