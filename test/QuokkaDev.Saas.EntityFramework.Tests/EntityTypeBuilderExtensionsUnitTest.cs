using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        [Fact(DisplayName = "Empty Tenant should produce no results")]
        public async Task Empty_Tenant_Should_Produce_No_Results()
        {
            // Arrange
            Tenant<int>? currentTenant = null;
            var tenantAccessorMock = new Mock<ITenantAccessor<Tenant<int>, int>>();
            tenantAccessorMock.Setup(m => m.Tenant).Returns(currentTenant);

            using TestDbContext context = TestDbContext.GetConfiguredContext(true, tenantAccessorMock.Object);

            // Act
            var result = await context.TestFilters.ToListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact(DisplayName = "ConfigureTenant should work properly")]
        public async Task ConfigureTenant_Should_Work_Properly()
        {
            // Arrange            

            DbContextOptionsBuilder<TestDbContext> optionsBuilder = new();
            optionsBuilder.UseInMemoryDatabase("temp", new InMemoryDatabaseRoot());

            // Act
            using TestDbContext context = new TestDbContext(optionsBuilder.Options, "_tenants", 1000);
            var entityType = context.Model.FindEntityType(typeof(Tenant<int>));

            // Assert
            entityType.Should().NotBeNull();
            entityType?.GetTableName().Should().Be("_tenants");
            var identifierProperty = entityType?.FindDeclaredProperty("Identifier");
            identifierProperty.Should().NotBeNull();
            identifierProperty?.GetAnnotation("MaxLength").Value.Should().Be(1000);
        }
    }
}
