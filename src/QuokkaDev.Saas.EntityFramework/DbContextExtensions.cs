using Microsoft.EntityFrameworkCore;
using QuokkaDev.Saas.Abstractions;

namespace QuokkaDev.Saas.EntityFramework
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Set the current tenant on new entities (added state) if not already set
        /// </summary>
        public static void SetTenant<T, TKey>(this DbContext context, ITenantAccessor<T, TKey> tenantAccessor) where T : Tenant<TKey>
        {
            var tenant = tenantAccessor.Tenant?.Identifier;

            foreach (var entity in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                var property = entity.Properties.FirstOrDefault(p => p.Metadata?.Name == "Tenant");
                if (property != null && string.IsNullOrEmpty(property.CurrentValue?.ToString()))
                {
                    property.CurrentValue = tenant;
                }
            }
        }
    }
}
