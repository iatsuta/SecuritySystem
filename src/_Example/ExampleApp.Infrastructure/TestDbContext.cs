using ExampleApp.Domain;
using ExampleApp.Domain.Auth.General;
using ExampleApp.Domain.Auth.Virtual;

using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Infrastructure;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    private const string DefaultIdPostfix = "Id";

    private const string DefaultSchema = "app";

    private const string AuthSchema = "auth";

	private const int DefaultMaxLength = 255;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
	    this.InitApp(modelBuilder);
	    this.InitAncestors(modelBuilder);
		this.InitVirtualPermission(modelBuilder);
        this.InitGeneralPermission(modelBuilder);

		base.OnModelCreating(modelBuilder);
	}

    private void InitApp(ModelBuilder modelBuilder)
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

            entity.Property(e => e.AllowedForFilterRole).IsRequired();
        }
	}

    private void InitAncestors(ModelBuilder modelBuilder)
	{
		{
			var entity = modelBuilder.Entity<BusinessUnitDirectAncestorLink>().ToTable(nameof(BusinessUnitDirectAncestorLink), DefaultSchema);
			entity.HasKey(v => v.Id);

			var ancestorKey = $"{nameof(BusinessUnitDirectAncestorLink.Ancestor)}{DefaultIdPostfix}";
			var childKey = $"{nameof(BusinessUnitDirectAncestorLink.Child)}{DefaultIdPostfix}";

			entity.HasOne(e => e.Ancestor).WithMany().HasForeignKey(ancestorKey).IsRequired();
			entity.HasOne(e => e.Child).WithMany().HasForeignKey(childKey).IsRequired();

			entity.HasIndex(ancestorKey, childKey).IsUnique();
		}

		{
			var entity = modelBuilder.Entity<BusinessUnitUndirectAncestorLink>().ToView(nameof(BusinessUnitUndirectAncestorLink), DefaultSchema);
			entity.HasNoKey();

			entity.HasOne(e => e.Source).WithMany().HasForeignKey($"{nameof(BusinessUnitUndirectAncestorLink.Source)}{DefaultIdPostfix}").IsRequired();
			entity.HasOne(e => e.Target).WithMany().HasForeignKey($"{nameof(BusinessUnitUndirectAncestorLink.Target)}{DefaultIdPostfix}").IsRequired();
		}
	}

	private void InitVirtualPermission(ModelBuilder modelBuilder)
    {
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
	}

    private void InitGeneralPermission(ModelBuilder modelBuilder)
	{
		{
			var entity = modelBuilder.Entity<SecurityContextType>().ToTable(nameof(SecurityContextType), AuthSchema);
			entity.HasKey(v => v.Id);
			entity.HasIndex(e => e.Name).IsUnique();

			entity.Property(e => e.Name).IsRequired().HasMaxLength(DefaultMaxLength);
		}

		{
			var entity = modelBuilder.Entity<PermissionRestriction>().ToTable(nameof(PermissionRestriction), AuthSchema);
			entity.HasKey(v => v.Id);

            var permissionKey = $"{nameof(PermissionRestriction.Permission)}{DefaultIdPostfix}";
            var securityContextTypeKey = $"{nameof(PermissionRestriction.SecurityContextType)}{DefaultIdPostfix}";

            entity.HasOne(e => e.Permission).WithMany().HasForeignKey(permissionKey).IsRequired();
            entity.HasOne(e => e.SecurityContextType).WithMany().HasForeignKey(securityContextTypeKey).IsRequired();
            entity.Property(e => e.SecurityContextId).IsRequired();


            entity.HasIndex(permissionKey, securityContextTypeKey, nameof(PermissionRestriction.SecurityContextId)).IsUnique();
        }

		{
		    var entity = modelBuilder.Entity<SecurityRole>().ToTable(nameof(SecurityRole), AuthSchema);
            entity.HasKey(v => v.Id);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.Name).IsRequired().HasMaxLength(DefaultMaxLength);

            entity.Property(e => e.Description).IsRequired().HasMaxLength(DefaultMaxLength);
        }

        {
            var entity = modelBuilder.Entity<Permission>().ToTable(nameof(Permission), AuthSchema);
            entity.HasKey(v => v.Id);

            entity.HasOne(e => e.SecurityRole).WithMany().HasForeignKey($"{nameof(Permission.SecurityRole)}{DefaultIdPostfix}").IsRequired();
            entity.HasOne(e => e.Principal).WithMany().HasForeignKey($"{nameof(Permission.Principal)}{DefaultIdPostfix}").IsRequired();

            entity.Property(e => e.Comment).IsRequired().HasMaxLength(DefaultMaxLength);
            entity.Property(e => e.ExtendedValue).IsRequired().HasMaxLength(DefaultMaxLength);
        }

        {
            var entity = modelBuilder.Entity<Principal>().ToTable(nameof(Principal), AuthSchema);
            entity.HasKey(v => v.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasOne(e => e.RunAs).WithMany().HasForeignKey($"{nameof(Principal.RunAs)}{DefaultIdPostfix}").IsRequired(false);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(DefaultMaxLength);
        }
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