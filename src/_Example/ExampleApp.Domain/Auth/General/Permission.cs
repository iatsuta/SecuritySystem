using SecuritySystem;

namespace ExampleApp.Domain.Auth.General;

public class Permission
{
    public Guid Id { get; init; }

    public virtual required Principal Principal { get; init; }

    public virtual required SecurityRole SecurityRole { get; init; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual void SetPeriod(PermissionPeriod period)
    {
        this.StartDate = period.StartDate ?? DateTime.MinValue;
        this.EndDate = period.EndDate;
    }
}