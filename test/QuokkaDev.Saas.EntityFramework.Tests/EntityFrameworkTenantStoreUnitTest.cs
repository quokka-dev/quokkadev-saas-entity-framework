using FluentAssertions;
using QuokkaDev.Saas.Abstractions;
using QuokkaDev.Saas.Abstractions.Exceptions;
using System.Threading.Tasks;
using Xunit;

namespace QuokkaDev.Saas.EntityFramework.Tests
{
    public class EntityFrameworkTenantStoreUnitTest
    {
        private readonly TestDbContext context;

        public EntityFrameworkTenantStoreUnitTest()
        {
            context = TestDbContext.GetConfiguredContext();
        }

        [Fact(DisplayName = "Store should work as expected")]
        public async Task Store_Should_Work_As_Expected()
        {
            // Arrange
            EntityFrameworkTenantStore<Tenant<int>, int> store = new(context);

            // Act
            var t1 = store.GetTenant("my-tenant-identifier");
            var t2 = await store.GetTenantAsync("my-tenant-identifier");
            var t3 = () => store.GetTenant("unknown");
            var t4 = async () => await store.GetTenantAsync("unknown");
            var t5 = store.GetTenant("alias2");
            var t6 = await store.GetTenantAsync("alias2");

            // Assert
            t1.Should().NotBeNull();
            t1.Identifier.Should().Be("my-tenant-identifier");
            t2.Should().NotBeNull();
            t2.Identifier.Should().Be("my-tenant-identifier");
            t3.Should().Throw<TenantNotFoundException>().Where(e => e.TenantIdentifier == "unknown");
            await t4.Should().ThrowAsync<TenantNotFoundException>().Where(e => e.TenantIdentifier == "unknown");
            t5.Should().NotBeNull();
            t5.Id.Should().Be(2);
            t6.Should().NotBeNull();
            t6.Id.Should().Be(2);
        }
    }
}
