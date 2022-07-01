using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QuokkaDev.Saas.Abstractions;
using QuokkaDev.Saas.DependencyInjection;
using System.Linq;
using Xunit;

namespace QuokkaDev.Saas.EntityFramework.Tests
{
    public class TenantBuilderExtensionsUnitTest
    {
        public TenantBuilderExtensionsUnitTest()
        {
        }

        [Fact(DisplayName = "Store should be registered")]
        public void Store_Should_Be_Registered()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            TenantBuilder<Tenant<int>, int> tenantBuilder = new(services);

            // Act
            tenantBuilder.WithEntityFrameworkStore();
            var store = services.FirstOrDefault(sd => sd.ServiceType == typeof(ITenantStore<Tenant<int>, int>));
            // Assert
            store.Should().NotBeNull();
            store?.ImplementationType.Should().NotBeNull();
            store?.ImplementationType.Should().Be(typeof(EntityFrameworkTenantStore<Tenant<int>, int>));
            store?.Lifetime.Should().Be(ServiceLifetime.Scoped);
            store?.ImplementationInstance.Should().BeNull();
        }
    }
}
