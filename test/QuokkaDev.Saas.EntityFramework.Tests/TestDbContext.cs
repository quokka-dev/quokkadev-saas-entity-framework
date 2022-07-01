using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuokkaDev.Saas.Abstractions;

namespace QuokkaDev.Saas.EntityFramework.Tests
{
    public class TestDbContext : DbContext
    {
        private readonly bool addTenantFilter;
        private readonly ITenantAccessor<Tenant<int>, int>? tenantAccessor;
        private readonly string? tenantsTableName;
        private readonly int tenantIdentifierMaxLength;

        public DbSet<Tenant<int>> Tenants { get; set; } = null!;
        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<TestFilter> TestFilters { get; set; } = null!;

        public TestDbContext()
        {
        }

        public TestDbContext(DbContextOptions<TestDbContext> context, string tenantsTableName, int tenantIdentifierMaxLength)
        {
            this.tenantsTableName = tenantsTableName;
            this.tenantIdentifierMaxLength = tenantIdentifierMaxLength;
        }

        public TestDbContext(DbContextOptions<TestDbContext> context, bool addTenantFilter = false, ITenantAccessor<Tenant<int>, int>? tenantAccessor = null) : base(context)
        {
            this.addTenantFilter = addTenantFilter;
            this.tenantAccessor = tenantAccessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("temp", new InMemoryDatabaseRoot());
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (!string.IsNullOrEmpty(tenantsTableName))
            {
                modelBuilder.ConfigureTenant<Tenant<int>, int>(tenantsTableName, tenantIdentifierMaxLength);
            }
            if (addTenantFilter)
            {
                modelBuilder.Entity<TestFilter>().AddPerTenantFilter(tenantAccessor!);
            }
            base.OnModelCreating(modelBuilder);
        }

        public static TestDbContext GetConfiguredContext(bool addTenantFilter = false, ITenantAccessor<Tenant<int>, int>? tenantAccessor = null)
        {
            DbContextOptionsBuilder<TestDbContext> optionsBuilder = new();
            optionsBuilder.UseInMemoryDatabase("temp", new InMemoryDatabaseRoot());
            var context = new TestDbContext(optionsBuilder.Options, addTenantFilter, tenantAccessor);
            context.Tenants.Add(new Tenant<int>(1, "my-tenant-identifier"));
            context.Tenants.Add(new Tenant<int>(2, "tenant-2") { Alias = "alias2" });
            context.Tenants.Add(new Tenant<int>(3, "tenant-3"));

            context.TestFilters.Add(new TestFilter() { Id = 1, Tenant = "my-tenant-identifier" });
            context.TestFilters.Add(new TestFilter() { Id = 2, Tenant = "my-tenant-identifier" });
            context.TestFilters.Add(new TestFilter() { Id = 3, Tenant = "my-tenant-identifier" });
            context.TestFilters.Add(new TestFilter() { Id = 4, Tenant = "my-tenant-identifier" });
            context.TestFilters.Add(new TestFilter() { Id = 5, Tenant = "my-tenant-identifier" });
            context.TestFilters.Add(new TestFilter() { Id = 6, Tenant = "other" });
            context.TestFilters.Add(new TestFilter() { Id = 7, Tenant = "other" });
            context.TestFilters.Add(new TestFilter() { Id = 8, Tenant = "other" });
            context.TestFilters.Add(new TestFilter() { Id = 9, Tenant = "other" });
            context.TestFilters.Add(new TestFilter() { Id = 10, Tenant = "other" });

            context.SaveChanges();
            return context;
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Tenant { get; set; }
    }

    public class TestFilter
    {
        public int Id { get; set; }
        public string? Tenant { get; set; }
    }
}
