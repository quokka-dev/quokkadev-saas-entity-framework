using Microsoft.Extensions.DependencyInjection;
using QuokkaDev.Saas.Abstractions;
using QuokkaDev.Saas.DependencyInjection;

namespace QuokkaDev.Saas.EntityFramework
{
    public static class TenantBuilderExtensions
    {
        public static TenantBuilder<T, TKey> WithEntityFrameworkStore<T, TKey>(this TenantBuilder<T, TKey> builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where T : Tenant<TKey>
        {
            builder.WithStore<EntityFrameworkTenantStore<T, TKey>>(lifetime);
            return builder;
        }
    }
}
