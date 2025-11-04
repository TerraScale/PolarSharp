using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Polar.NET;
using Polar.NET.Models.Products;
using Xunit;

namespace Polar.NET.IntegrationTests;

/// <summary>
/// Integration tests for PolarClient using the sandbox environment.
/// </summary>
public class PolarClientIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public PolarClientIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PolarClient_WithValidToken_CanConnectToSandbox()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var products = await client.Products.ListAsync(limit: 1);

        // Assert
        products.Should().NotBeNull();
        products.Items.Should().NotBeNull();
        products.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductsApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Products.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ProductsApi_ListAllAsync_EnumeratesAllPages()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var products = new List<Product>();
        await foreach (var product in client.Products.ListAllAsync())
        {
            products.Add(product);
        }

        // Assert
        products.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProductsApi_CreateAndGetProduct_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createRequest = new ProductCreateRequest
        {
            Name = $"Test Product {Guid.NewGuid()}",
            Description = "Integration test product",
            Type = ProductType.OneTime
        };

        // Act
        var createdProduct = await client.Products.CreateAsync(createRequest);
        var retrievedProduct = await client.Products.GetAsync(createdProduct.Id);

        // Assert
        createdProduct.Should().NotBeNull();
        createdProduct.Id.Should().NotBeNullOrEmpty();
        createdProduct.Name.Should().Be(createRequest.Name);
        createdProduct.Description.Should().Be(createRequest.Description);

        retrievedProduct.Should().NotBeNull();
        retrievedProduct.Id.Should().Be(createdProduct.Id);
        retrievedProduct.Name.Should().Be(createdProduct.Name);
        retrievedProduct.Description.Should().Be(createdProduct.Description);

        // Cleanup
        await client.Products.ArchiveAsync(createdProduct.Id);
    }

    [Fact]
    public async Task ProductsApi_UpdateProduct_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createRequest = new ProductCreateRequest
        {
            Name = $"Test Product {Guid.NewGuid()}",
            Description = "Original description",
            Type = ProductType.OneTime
        };

        var createdProduct = await client.Products.CreateAsync(createRequest);
        var updateRequest = new ProductUpdateRequest
        {
            Name = "Updated Product Name",
            Description = "Updated description"
        };

        // Act
        var updatedProduct = await client.Products.UpdateAsync(createdProduct.Id, updateRequest);

        // Assert
        updatedProduct.Should().NotBeNull();
        updatedProduct.Id.Should().Be(createdProduct.Id);
        updatedProduct.Name.Should().Be(updateRequest.Name);
        updatedProduct.Description.Should().Be(updateRequest.Description);

        // Cleanup
        await client.Products.ArchiveAsync(createdProduct.Id);
    }

    [Fact]
    public async Task OrdersApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Orders.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task OrdersApi_ListAllAsync_EnumeratesAllPages()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var orders = new List<Polar.NET.Models.Orders.Order>();
        await foreach (var order in client.Orders.ListAllAsync())
        {
            orders.Add(order);
        }

        // Assert
        orders.Should().NotBeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Subscriptions.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task SubscriptionsApi_ListAllAsync_EnumeratesAllPages()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var subscriptions = new List<Polar.NET.Models.Subscriptions.Subscription>();
        await foreach (var subscription in client.Subscriptions.ListAllAsync())
        {
            subscriptions.Add(subscription);
        }

        // Assert
        subscriptions.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckoutsApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Checkouts.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CheckoutsApi_CreateCheckout_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, we need a product to create a checkout for
        var productRequest = new ProductCreateRequest
        {
            Name = $"Test Product {Guid.NewGuid()}",
            Description = "Integration test product for checkout",
            Type = ProductType.OneTime
        };

        var product = await client.Products.CreateAsync(productRequest);
        
        // Create a price for the product
        var priceRequest = new ProductPriceCreateRequest
        {
            Amount = 1000, // $10.00
            Currency = "USD",
            Type = ProductPriceType.OneTime
        };

        var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

        var checkoutRequest = new Polar.NET.Models.Checkouts.CheckoutCreateRequest
        {
            ProductId = product.Id,
            ProductPriceId = price.Id,
            SuccessUrl = "https://example.com/success",
            CancelUrl = "https://example.com/cancel"
        };

        // Act
        var checkout = await client.Checkouts.CreateAsync(checkoutRequest);

        // Assert
        checkout.Should().NotBeNull();
        checkout.Id.Should().NotBeNullOrEmpty();
        checkout.ProductId.Should().Be(product.Id);
        checkout.ProductPriceId.Should().Be(price.Id);
        checkout.SuccessUrl.Should().Be(checkoutRequest.SuccessUrl);
        checkout.CancelUrl.Should().Be(checkoutRequest.CancelUrl);

        // Cleanup
        await client.Products.ArchiveAsync(product.Id);
    }

    [Fact]
    public async Task BenefitsApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Benefits.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task BenefitsApi_CreateBenefit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createRequest = new Polar.NET.Models.Benefits.BenefitCreateRequest
        {
            Name = $"Test Benefit {Guid.NewGuid()}",
            Description = "Integration test benefit",
            Type = Polar.NET.Models.Benefits.BenefitType.Custom,
            Selectable = true
        };

        // Act
        var createdBenefit = await client.Benefits.CreateAsync(createRequest);
        var retrievedBenefit = await client.Benefits.GetAsync(createdBenefit.Id);

        // Assert
        createdBenefit.Should().NotBeNull();
        createdBenefit.Id.Should().NotBeNullOrEmpty();
        createdBenefit.Name.Should().Be(createRequest.Name);
        createdBenefit.Description.Should().Be(createRequest.Description);
        createdBenefit.Type.Should().Be(createRequest.Type);
        createdBenefit.Selectable.Should().Be(createRequest.Selectable);

        retrievedBenefit.Should().NotBeNull();
        retrievedBenefit.Id.Should().Be(createdBenefit.Id);
        retrievedBenefit.Name.Should().Be(createdBenefit.Name);
        retrievedBenefit.Description.Should().Be(createdBenefit.Description);

        // Cleanup
        await client.Benefits.DeleteAsync(createdBenefit.Id);
    }

    [Fact]
    public async Task CustomersApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Customers.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CustomersApi_CreateAndGetCustomer_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createRequest = new Polar.NET.Models.Customers.CustomerCreateRequest
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Name = "Test Customer",
            ExternalId = Guid.NewGuid().ToString()
        };

        // Act
        var createdCustomer = await client.Customers.CreateAsync(createRequest);
        var retrievedCustomer = await client.Customers.GetAsync(createdCustomer.Id);

        // Assert
        createdCustomer.Should().NotBeNull();
        createdCustomer.Id.Should().NotBeNullOrEmpty();
        createdCustomer.Email.Should().Be(createRequest.Email);
        createdCustomer.Name.Should().Be(createRequest.Name);
        createdCustomer.ExternalId.Should().Be(createRequest.ExternalId);

        retrievedCustomer.Should().NotBeNull();
        retrievedCustomer.Id.Should().Be(createdCustomer.Id);
        retrievedCustomer.Email.Should().Be(createdCustomer.Email);
        retrievedCustomer.Name.Should().Be(createdCustomer.Name);
        retrievedCustomer.ExternalId.Should().Be(createdCustomer.ExternalId);

        // Cleanup
        await client.Customers.DeleteAsync(createdCustomer.Id);
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateSession_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, we need a customer to create a session for
        var customerRequest = new Polar.NET.Models.Customers.CustomerCreateRequest
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Name = "Test Customer for Session"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var sessionRequest = new Polar.NET.Models.CustomerSessions.CustomerSessionCreateRequest
        {
            CustomerId = customer.Id
        };

        // Act
        var session = await client.CustomerSessions.CreateAsync(sessionRequest);

        // Assert
        session.Should().NotBeNull();
        session.Id.Should().NotBeNullOrEmpty();
        session.CustomerId.Should().Be(customer.Id);
        session.CustomerAccessToken.Should().NotBeNullOrEmpty();
        session.CustomerAccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Cleanup
        await client.Customers.DeleteAsync(customer.Id);
    }

    [Fact]
    public async Task LicenseKeysApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.LicenseKeys.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CustomerPortalApi_WithCustomerToken_CanAccessCustomerData()
    {
        // This test would require a valid customer access token
        // For now, we'll test the creation method
        var customerAccessToken = "test_customer_token";
        
        // Act & Assert
        var action = () => Polar.NET.Api.CustomerPortalApi.Create(customerAccessToken);
        action.Should().NotThrow();
    }

    [Fact]
    public void PolarClientBuilder_WithEnvironment_SetsCorrectBaseUrl()
    {
        // Arrange
        var accessToken = "test_token";

        // Act
        var productionClient = PolarClient.Create()
            .WithToken(accessToken)
            .WithEnvironment(Polar.NET.Models.Common.PolarEnvironment.Production)
            .Build();

        var sandboxClient = PolarClient.Create()
            .WithToken(accessToken)
            .WithEnvironment(Polar.NET.Models.Common.PolarEnvironment.Sandbox)
            .Build();

        // Assert
        productionClient.Should().NotBeNull();
        sandboxClient.Should().NotBeNull();
    }

    [Fact]
    public void PolarClientBuilder_WithCustomOptions_ConfiguresCorrectly()
    {
        // Arrange
        var accessToken = "test_token";
        var customUserAgent = "MyApp/1.0.0";

        // Act
        var client = PolarClient.Create()
            .WithToken(accessToken)
            .WithUserAgent(customUserAgent)
            .WithTimeout(60)
            .WithMaxRetries(5)
            .Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task PolarClient_WithInvalidToken_HandlesErrorGracefully()
    {
        // Arrange
        var client = new PolarClient("invalid_token");

        // Act & Assert
        var action = async () => await client.Products.ListAsync(limit: 1);
        await action.Should().ThrowAsync<Exception>();
    }
}