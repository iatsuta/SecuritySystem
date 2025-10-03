using ExampleApp.Domain;
using ExampleApp.Domain.Auth;

using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Infrastructure;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    private const string DefaultIdPostfix = "Id";

    private const string DefaultSchema = "app";

    private const int DefaultMaxLength = 255;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        {
            var entity = modelBuilder.Entity<TestObject>().ToTable(nameof(TestObject), DefaultSchema);
            entity.HasKey(v => v.Id);

            entity.HasOne(e => e.BusinessUnit).WithMany().HasForeignKey($"{nameof(TestObject.BusinessUnit)}{DefaultIdPostfix}").IsRequired();
            entity.HasOne(e => e.Location).WithMany().HasForeignKey($"{nameof(TestObject.Location)}{DefaultIdPostfix}").IsRequired();
        }

        {
            var entity = modelBuilder.Entity<Employee>().ToTable(nameof(Employee), DefaultSchema);
            entity.HasKey(v => v.Id);
            entity.HasIndex(e => e.Login).IsUnique();

            entity.Property(e => e.Login).IsRequired().HasMaxLength(DefaultMaxLength);
            entity.HasOne(e => e.RunAs).WithMany().HasForeignKey($"{nameof(Employee.RunAs)}{DefaultIdPostfix}").IsRequired(false);
        }

        {
            var entity = modelBuilder.Entity<Administrator>().ToTable(nameof(Administrator), DefaultSchema);
            entity.HasKey(v => v.Id);

            entity.HasOne(e => e.Employee).WithMany().HasForeignKey($"{nameof(Administrator.Employee)}{DefaultIdPostfix}").IsRequired();
        }

        {
            var entity = modelBuilder.Entity<TestManager>().ToTable(nameof(TestManager), DefaultSchema);
            entity.HasKey(v => v.Id);

            entity.HasOne(e => e.Employee).WithMany().HasForeignKey($"{nameof(TestManager.Employee)}{DefaultIdPostfix}").IsRequired();
            entity.HasOne(e => e.Location).WithMany().HasForeignKey($"{nameof(TestManager.Location)}{DefaultIdPostfix}").IsRequired();
            entity.HasOne(e => e.BusinessUnit).WithMany().HasForeignKey($"{nameof(TestManager.BusinessUnit)}{DefaultIdPostfix}").IsRequired();
        }
        {
            var entity = modelBuilder.Entity<Location>().ToTable(nameof(Location), DefaultSchema);
            entity.HasKey(v => v.MyId);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(DefaultMaxLength);
        }

        {
            var entity = modelBuilder.Entity<BusinessUnit>().ToTable(nameof(BusinessUnit), DefaultSchema);
            entity.HasKey(v => v.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(DefaultMaxLength);
            entity.HasOne(e => e.Parent).WithMany().HasForeignKey($"{nameof(BusinessUnit.Parent)}{DefaultIdPostfix}").IsRequired(false);
        }

        {
            var entity = modelBuilder.Entity<BusinessUnitDirectAncestorLink>().ToTable(nameof(BusinessUnitDirectAncestorLink), DefaultSchema);
            entity.HasKey(v => v.Id);

            entity.HasOne(e => e.Ancestor).WithMany().HasForeignKey($"{nameof(BusinessUnitDirectAncestorLink.Ancestor)}{DefaultIdPostfix}").IsRequired();
            entity.HasOne(e => e.Child).WithMany().HasForeignKey($"{nameof(BusinessUnitDirectAncestorLink.Child)}{DefaultIdPostfix}").IsRequired();
        }

        {
            var entity = modelBuilder.Entity<BusinessUnitUndirectAncestorLink>().ToView(nameof(BusinessUnitUndirectAncestorLink), DefaultSchema);
            entity.HasNoKey();

            entity.HasOne(e => e.Source).WithMany().HasForeignKey($"{nameof(BusinessUnitUndirectAncestorLink.Source)}{DefaultIdPostfix}").IsRequired();
            entity.HasOne(e => e.Target).WithMany().HasForeignKey($"{nameof(BusinessUnitUndirectAncestorLink.Target)}{DefaultIdPostfix}").IsRequired();
        }

        base.OnModelCreating(modelBuilder);
    }

    public async Task EnsureViewsCreatedAsync(CancellationToken cancellationToken = default)
    {
        await Database.ExecuteSqlRawAsync(@$"
CREATE VIEW {nameof(BusinessUnitUndirectAncestorLink)}
AS
SELECT ancestorId as sourceId, childId as targetId 
FROM {nameof(BusinessUnitDirectAncestorLink)}
UNION
SELECT childId as sourceId, ancestorId as targetId
FROM {nameof(BusinessUnitDirectAncestorLink)}
", cancellationToken);
    }
}