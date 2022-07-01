using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuokkaDev.Saas.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace QuokkaDev.Saas.EntityFramework.Tests
{
    public class EntityTypeBuilderExtensionsUnitTest
    {
        public EntityTypeBuilderExtensionsUnitTest()
        {
        }

        [Theory(DisplayName = "Tenant filter should be applied")]
        [InlineData(true, 5)]
        [InlineData(false, 10)]
        public async Task Tenant_Filter_Should_Be_Applied(bool applyFilter, int expectedCount)
        {
            // Arrange
            var currentTenant = new Tenant<int>(1, "my-tenant-identifier");
            var tenantAccessorMock = new Mock<ITenantAccessor<Tenant<int>, int>>();
            tenantAccessorMock.Setup(m => m.Tenant).Returns(currentTenant);

            using TestDbContext context = TestDbContext.GetConfiguredContext(applyFilter, tenantAccessorMock.Object);

            // Act
            var result = await context.TestFilters.ToListAsync();

            // Assert
            result.Should().HaveCount(expectedCount);
        }

    }
}
