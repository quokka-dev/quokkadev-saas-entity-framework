using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuokkaDev.Saas.Abstractions;
using System.Linq.Expressions;
using System.Reflection;

namespace QuokkaDev.Saas.EntityFramework
{
    public static class EntityTypeBuilderExtensions
    {
        private static readonly MethodInfo _propertyMethod = typeof(EF)!.GetMethod(nameof(EF.Property), BindingFlags.Static |
            BindingFlags.Public)!.MakeGenericMethod(typeof(string));

        /// <summary>
        /// Add a tenant filter for all the entities in a table
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// /// <typeparam name="TKey">Type of the entity</typeparam>
        /// <param name="entityBuilder"></param>
        /// <param name="tenant"></param>
        /// <param name="tenantAccessorExpression"></param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> AddPerTenantFilter<TEntity, TTenant, TKey>(this EntityTypeBuilder<TEntity> entityBuilder, ITenantAccessor<TTenant, TKey> tenantAccessor)
            where TEntity : class
            where TTenant : Tenant<TKey>
        {
            entityBuilder.Property<string>("Tenant");
            string? currentTenantIdentifier = tenantAccessor.Tenant?.Identifier;

            entityBuilder.HasQueryFilter(GetTenantRestrictionLambda(typeof(TEntity), currentTenantIdentifier));

            return entityBuilder;
        }

        /// <summary>
        /// Configure CustomerPortalTenant entity
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ConfigureTenant<T, TKey>(this ModelBuilder modelBuilder, string tableName = "Tenants", int identifierMaxLength = 50) where T : Tenant<TKey>
        {
            modelBuilder.Entity<T>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable(tableName);
                entity.Property(e => e.Identifier)
                    .HasMaxLength(identifierMaxLength)
                    .IsRequired();
                entity.Ignore(t => t.Items);
                entity.HasIndex(t => t.Identifier).IsUnique();
            });
        }

        private static LambdaExpression GetTenantRestrictionLambda(Type type, string? currentTenantIdentifier)
        {
            var parameter = Expression.Parameter(type, "it");
            var prop = Expression.Call(_propertyMethod, parameter, Expression.Constant("Tenant"));
            var condition = Expression.MakeBinary(ExpressionType.Equal, prop, Expression.Constant(currentTenantIdentifier));
            var lambda = Expression.Lambda(condition, parameter);

            return lambda;
        }
    }
}
