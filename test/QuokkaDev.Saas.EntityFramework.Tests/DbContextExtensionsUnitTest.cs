using FluentAssertions;
using Moq;
using QuokkaDev.Saas.Abstractions;
using System;
using System.Linq;
using Xunit;

namespace QuokkaDev.Saas.EntityFramework.Tests;

public class DbContextExtensionsUnitTest
{
    private readonly TestDbContext context;
    private readonly ITenantAccessor<Tenant<int>, int> tenantAccessor;
    public DbContextExtensionsUnitTest()
    {
        context = TestDbContext.GetConfiguredContext();
        var currentTenant = context.Tenants.First();

        var tenantAccessorMock = new Mock<ITenantAccessor<Tenant<int>, int>>();
        tenantAccessorMock.Setup(m => m.Tenant).Returns(currentTenant);

        tenantAccessor = tenantAccessorMock.Object;
    }

    [Fact(DisplayName = "Tenant property is configured on new entities")]
    public void Tenant_Property_Is_Configured_On_New_Entities()
    {
        // Arrange 
        Person p1 = new() { Id = 1, Name = "Joe" };
        Person p2 = new() { Id = 2, Name = "Jack", Tenant = "another-tenant" };

        // Act
        context.People.Add(p1);
        context.People.Add(p2);

        context.SetTenant(tenantAccessor);
        context.SaveChanges();

        // Assert
        p1.Tenant.Should().Be("my-tenant-identifier");
        p2.Tenant.Should().Be("another-tenant");
    }

    [Fact(DisplayName = "New entities without Tenant property are unaffected")]
    public void New_Entities_Without_Tenant_Property_Are_Unaffected()
    {
        // Arrange 
        Order o = new() { Id = 1, CustomerName = "IBM" };

        // Act
        context.Orders.Add(o);
        Action setTenantInvocation = () => context.SetTenant(tenantAccessor);

        // Assert
        setTenantInvocation.Should().NotThrow<Exception>();
    }

    [Fact(DisplayName = "Modified entities are unaffected")]
    public void Modified_Entities_Are_Unaffected()
    {
        // Arrange
        Person p1 = new() { Id = 3, Name = "Mike", Tenant = "my-tenant-identifier" };

        // Act
        context.People.Add(p1);
        context.SetTenant(tenantAccessor);
        context.SaveChanges();

        p1.Tenant = null;
        context.SetTenant(tenantAccessor);
        context.SaveChanges();

        // Assert
        p1.Tenant.Should().BeNull();
    }

    [Fact(DisplayName = "Empty tenant should throw exception")]
    public void Empty_Tenant_Should_Throw_Exception()
    {
        // Arrange
        Person p1 = new() { Id = 3, Name = "Mike" };

        var tenantAccessorMock = new Mock<ITenantAccessor<Tenant<int>, int>>();
        tenantAccessorMock.Setup(m => m.Tenant).Returns((Tenant<int>?)null);

        // Act
        context.People.Add(p1);
        var setTenantInvocation = () => context.SetTenant(tenantAccessorMock.Object);

        // Assert
        setTenantInvocation.Should().Throw<ArgumentNullException>();
    }
}