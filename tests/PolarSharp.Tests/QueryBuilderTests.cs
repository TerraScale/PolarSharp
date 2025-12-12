using FluentAssertions;
using PolarSharp.Extensions;
using Xunit;

namespace PolarSharp.Tests;

public class ProductsQueryBuilderTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmpty()
    {
        // Arrange & Act
        var builder = new ProductsQueryBuilder();

        // Assert
        builder.Build().Should().BeEmpty();
    }

    [Fact]
    public void WithActive_WithTrue_ShouldAddParameter()
    {
        // Arrange
        var builder = new ProductsQueryBuilder();

        // Act
        builder.WithActive(true);

        // Assert
        var result = builder.Build();
        result.Should().Be("is_active=true");
    }

    [Fact]
    public void CreatedBefore_WithNull_ShouldNotAddParameter()
    {
        // Arrange
        var builder = new ProductsQueryBuilder();

        // Act
        builder.CreatedBefore(null);

        // Assert
        builder.Build().Should().BeEmpty();
    }

    [Fact]
    public void ChainedMethods_ShouldBuildCorrectQuery()
    {
        // Arrange
        var builder = new ProductsQueryBuilder();
        var createdAfter = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var createdBefore = new DateTime(2023, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        var result = builder
            .WithActive(true)
            .WithType("license")
            .CreatedAfter(createdAfter)
            .CreatedBefore(createdBefore)
            .Build();

        // Assert
        result.Should().Contain("is_active=true");
        result.Should().Contain("type=license");
        result.Should().Contain("created_after=2023-01-01T00%3A00%3A00Z");
        result.Should().Contain("created_before=2023-12-31T23%3A59%3A59Z");
    }

    [Fact]
    public void GetParameters_ShouldReturnAllParameters()
    {
        // Arrange
        var builder = new ProductsQueryBuilder();
        builder.WithActive(true).WithType("license");

        // Act
        var parameters = builder.GetParameters();

        // Assert
        parameters.Should().ContainKey("is_active");
        parameters.Should().ContainKey("type");
        parameters["is_active"].Should().Be("true");
        parameters["type"].Should().Be("license");
    }
}

public class OrdersQueryBuilderTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmpty()
    {
        // Arrange & Act
        var builder = new OrdersQueryBuilder();

        // Assert
        builder.Build().Should().BeEmpty();
    }

    [Fact]
    public void WithStatus_ShouldAddParameter()
    {
        // Arrange
        var builder = new OrdersQueryBuilder();

        // Act
        builder.WithStatus("COMPLETED");

        // Assert
        var result = builder.Build();
        result.Should().Be("status=completed");
    }

    [Fact]
    public void WithStatus_WithNull_ShouldNotAddParameter()
    {
        // Arrange
        var builder = new OrdersQueryBuilder();

        // Act
        builder.WithStatus(null);

        // Assert
        builder.Build().Should().BeEmpty();
    }

    [Fact]
    public void WithCustomerId_ShouldAddParameter()
    {
        // Arrange
        var builder = new OrdersQueryBuilder();

        // Act
        builder.WithCustomerId("cust_123");

        // Assert
        var result = builder.Build();
        result.Should().Be("customer_id=cust_123");
    }

    [Fact]
    public void WithProductId_ShouldAddParameter()
    {
        // Arrange
        var builder = new OrdersQueryBuilder();

        // Act
        builder.WithProductId("prod_456");

        // Assert
        var result = builder.Build();
        result.Should().Be("product_id=prod_456");
    }

    [Fact]
    public void CreatedAfter_ShouldAddParameter()
    {
        // Arrange
        var builder = new OrdersQueryBuilder();
        var date = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);

        // Act
        builder.CreatedAfter(date);

        // Assert
        var result = builder.Build();
        result.Should().Be("created_after=2023-12-25T10%3A30%3A00Z");
    }

    [Fact]
    public void CreatedBefore_ShouldAddParameter()
    {
        // Arrange
        var builder = new OrdersQueryBuilder();
        var date = new DateTime(2023, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        builder.CreatedBefore(date);

        // Assert
        var result = builder.Build();
        result.Should().Be("created_before=2023-12-31T23%3A59%3A59Z");
    }

    [Fact]
    public void ChainedMethods_ShouldBuildCorrectQuery()
    {
        // Arrange
        var builder = new OrdersQueryBuilder();
        var createdAfter = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = builder
            .WithStatus("COMPLETED")
            .WithCustomerId("cust_123")
            .WithProductId("prod_456")
            .CreatedAfter(createdAfter)
            .Build();

        // Assert
        result.Should().Contain("status=completed");
        result.Should().Contain("customer_id=cust_123");
        result.Should().Contain("product_id=prod_456");
        result.Should().Contain("created_after=2023-01-01T00%3A00%3A00Z");
    }
}

public class SubscriptionsQueryBuilderTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmpty()
    {
        // Arrange & Act
        var builder = new SubscriptionsQueryBuilder();

        // Assert
        builder.Build().Should().BeEmpty();
    }

    [Fact]
    public void WithStatus_ShouldAddParameter()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        builder.WithStatus("ACTIVE");

        // Assert
        var result = builder.Build();
        result.Should().Be("status=active");
    }

    [Fact]
    public void WithCustomerId_ShouldAddParameter()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        builder.WithCustomerId("cust_123");

        // Assert
        var result = builder.Build();
        result.Should().Be("customer_id=cust_123");
    }

    [Fact]
    public void WithProductId_ShouldAddParameter()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        builder.WithProductId("prod_456");

        // Assert
        var result = builder.Build();
        result.Should().Be("product_id=prod_456");
    }

    [Fact]
    public void WithCanceled_WithTrue_ShouldAddParameter()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        builder.WithCanceled(true);

        // Assert
        var result = builder.Build();
        result.Should().Be("canceled=true");
    }

    [Fact]
    public void WithCanceled_WithFalse_ShouldAddParameter()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        builder.WithCanceled(false);

        // Assert
        var result = builder.Build();
        result.Should().Be("canceled=false");
    }

    [Fact]
    public void WithCanceled_WithNull_ShouldNotAddParameter()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        builder.WithCanceled(null);

        // Assert
        builder.Build().Should().BeEmpty();
    }
    
    [Fact]
    public void WithExternalId_ShouldAddParameter()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        builder.WithExternalId("ext_789");

        // Assert
        var result = builder.Build();
        result.Should().Be("external_customer_id=ext_789");
    }

    [Fact]
    public void ChainedMethods_ShouldBuildCorrectQuery()
    {
        // Arrange
        var builder = new SubscriptionsQueryBuilder();

        // Act
        var result = builder
            .WithStatus("ACTIVE")
            .WithCustomerId("cust_123")
            .WithProductId("prod_456")
            .WithExternalId("ext_789")
            .WithCanceled(false)
            .Build();

        // Assert
        result.Should().Contain("status=active");
        result.Should().Contain("customer_id=cust_123");
        result.Should().Contain("product_id=prod_456");
        result.Should().Contain("external_customer_id=ext_789");
        result.Should().Contain("canceled=false");
    }
}

public class CustomersQueryBuilderTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmpty()
    {
        // Arrange & Act
        var builder = new CustomersQueryBuilder();

        // Assert
        builder.Build().Should().BeEmpty();
    }

    [Fact]
    public void WithEmail_ShouldAddParameter()
    {
        // Arrange
        var builder = new CustomersQueryBuilder();

        // Act
        builder.WithEmail("test@example.com");

        // Assert
        var result = builder.Build();
        result.Should().Be("email=test%40example.com");
    }

    [Fact]
    public void WithExternalId_ShouldAddParameter()
    {
        // Arrange
        var builder = new CustomersQueryBuilder();

        // Act
        builder.WithExternalId("ext_123");

        // Assert
        var result = builder.Build();
        result.Should().Be("external_id=ext_123");
    }

    [Fact]
    public void CreatedAfter_ShouldAddParameter()
    {
        // Arrange
        var builder = new CustomersQueryBuilder();
        var date = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);

        // Act
        builder.CreatedAfter(date);

        // Assert
        var result = builder.Build();
        result.Should().Be("created_after=2023-12-25T10%3A30%3A00Z");
    }

    [Fact]
    public void CreatedBefore_ShouldAddParameter()
    {
        // Arrange
        var builder = new CustomersQueryBuilder();
        var date = new DateTime(2023, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        builder.CreatedBefore(date);

        // Assert
        var result = builder.Build();
        result.Should().Be("created_before=2023-12-31T23%3A59%3A59Z");
    }

    [Fact]
    public void ChainedMethods_ShouldBuildCorrectQuery()
    {
        // Arrange
        var builder = new CustomersQueryBuilder();
        var createdAfter = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = builder
            .WithEmail("test@example.com")
            .WithExternalId("ext_123")
            .CreatedAfter(createdAfter)
            .Build();

        // Assert
        result.Should().Contain("email=test%40example.com");
        result.Should().Contain("external_id=ext_123");
        result.Should().Contain("created_after=2023-01-01T00%3A00%3A00Z");
    }

    [Fact]
    public void WithEmail_WithSpecialCharacters_ShouldEncodeCorrectly()
    {
        // Arrange
        var builder = new CustomersQueryBuilder();

        // Act
        builder.WithEmail("test+user@example.co.uk");

        // Assert
        var result = builder.Build();
        result.Should().Be("email=test%2Buser%40example.co.uk");
    }
}