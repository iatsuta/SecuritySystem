using ExampleWebApp.Domain;
using ExampleWebApp.Domain.Auth;

using Microsoft.EntityFrameworkCore;

namespace ExampleWebApp.Infrastructure;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestObject>();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Login)
            .IsUnique();

        modelBuilder.Entity<BusinessUnit>();

        modelBuilder.Entity<Administrator>();

        modelBuilder.Entity<TestManager>();

        modelBuilder.Entity<Location>();

        base.OnModelCreating(modelBuilder);
    }
}