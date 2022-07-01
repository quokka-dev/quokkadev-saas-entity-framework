using Microsoft.EntityFrameworkCore;
using QuokkaDev.Saas.Abstractions;
using QuokkaDev.Saas.Abstractions.Exceptions;

namespace QuokkaDev.Saas.EntityFramework
{
    public class EntityFrameworkTenantStore<TTenant, TKey> : ITenantStore<TTenant, TKey> where TTenant : Tenant<TKey>
    {
        private readonly DbContext context;

        public EntityFrameworkTenantStore(DbContext context)
        {
            this.context = context;
        }

        public TTenant GetTenant(string identifier)
        {
            TTenant? t = null;

            var set = context.Set<TTenant>();
            if (set != null)
            {
                t = set.FirstOrDefault(t => t.Identifier == identifier) ?? set.FirstOrDefault(t => ("" + t.Alias).Contains(identifier));
            }

            return t ?? throw new TenantNotFoundException() { TenantIdentifier = identifier };
        }

        public async Task<TTenant> GetTenantAsync(string identifier)
        {
            TTenant? t = null;

            var set = context.Set<TTenant>();
            if (set != null)
            {
                t = await set.FirstOrDefaultAsync(t => t.Identifier == identifier) ?? await set.FirstOrDefaultAsync(t => ("" + t.Alias).Contains(identifier));
            }

            return t ?? throw new TenantNotFoundException() { TenantIdentifier = identifier };
        }
    }
}
