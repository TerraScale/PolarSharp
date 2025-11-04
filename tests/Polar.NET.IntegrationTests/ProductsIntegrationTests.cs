using FluentAssertions;
using Polar.NET.Models.Products;
using Xunit;

namespace Polar.NET.IntegrationTests;

/// <summary>
/// Integration tests for Products API.
/// </summary>
public class ProductsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public ProductsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProductsApi_CreateProductWithPrices_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var priceRequest1 = new ProductPriceCreateRequest
        {
            Amount = 1000, // $10.00 in cents
            Currency = "USD",
            Type = ProductPriceType.OneTime
        };
        
        var priceRequest2 = new ProductPriceCreateRequest
        {
            Amount = 2000, // $20.00 in cents
            Currency = "EUR",
            Type = ProductPriceType.OneTime
        };

        var createRequest = new ProductCreateRequest
        {
            Name = $"Test Product {Guid.NewGuid()}",
            Description = "Integration test product with prices",
            Type = ProductType.OneTime,
            Prices = new List<ProductPriceCreateRequest> { priceRequest1, priceRequest2 }
        };

        // Act
        var createdProduct = await client.Products.CreateAsync(createRequest);

        // Retrieve product with prices
        var retrievedProduct = await client.Products.GetAsync(createdProduct.Id);

        // Assert
        createdProduct.Should().NotBeNull();
        createdProduct.Id.Should().NotBeNullOrEmpty();
        createdProduct.Name.Should().Be(createRequest.Name);
        createdProduct.Description.Should().Be(createRequest.Description);

        retrievedProduct.Should().NotBeNull();
        retrievedProduct.Prices.Should().HaveCount(2);

        // Cleanup
        await client.Products.ArchiveAsync(createdProduct.Id);
    }

    [Fact]
    public async Task ProductsApi_CreateSubscriptionProduct_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var productRequest = new ProductCreateRequest
        {
            Name = $"Test Subscription Product {Guid.NewGuid()}",
            Description = "A test subscription product",
            Type = ProductType.Subscription,
            RecurringInterval = RecurringInterval.Month,
            Prices = new List<ProductPriceCreateRequest>
            {
                new ProductPriceCreateRequest
                {
                    Amount = 1999, // $19.99 in cents
                    Currency = "USD",
                    Type = ProductPriceType.Recurring,
                    RecurringInterval = "month"
                }
            }
        };

        // Act
        var createdProduct = await client.Products.CreateAsync(productRequest);

        // Assert
        createdProduct.Should().NotBeNull();
        createdProduct.Type.Should().Be(ProductType.Subscription);
        createdProduct.IsSubscription.Should().BeTrue();
        createdProduct.Prices.Should().NotBeEmpty();
        createdProduct.Prices.First().Type.Should().Be(ProductPriceType.Recurring);
        createdProduct.Prices.First().RecurringInterval.Should().Be(RecurringInterval.Month);

        // Cleanup
        await client.Products.ArchiveAsync(createdProduct.Id);
    }

    [Fact]
    public async Task ProductsApi_QueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var query = client.Products.Query()
            .WithActive(true)
            .WithType("one_time");

        var result = await client.Products.ListAsync(query, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductsApi_Export_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var exportResult = await client.Products.ExportAsync();

        // Assert
        exportResult.Should().NotBeNull();
        exportResult.ExportUrl.Should().NotBeNullOrEmpty();
        exportResult.Size.Should().BeGreaterThan(0);
        exportResult.RecordCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ProductsApi_ExportPrices_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First create a product to export prices for
        var productRequest = new ProductCreateRequest
        {
            Name = $"Test Product {Guid.NewGuid()}",
            Type = ProductType.OneTime,
            Prices = new List<ProductPriceCreateRequest>
            {
                new ProductPriceCreateRequest
                {
                    Amount = 999, // $9.99 in cents
                    Currency = "USD",
                    Type = ProductPriceType.OneTime
                }
            }
        };

        var product = await client.Products.CreateAsync(productRequest);

        // Act
        var exportResult = await client.Products.ExportPricesAsync(product.Id);

        // Assert
        exportResult.Should().NotBeNull();
        exportResult.ExportUrl.Should().NotBeNullOrEmpty();
        exportResult.Size.Should().BeGreaterThan(0);
        exportResult.RecordCount.Should().BeGreaterOrEqualTo(0);

        // Cleanup
        await client.Products.ArchiveAsync(product.Id);
    }
}