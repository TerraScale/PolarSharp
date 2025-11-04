using FluentAssertions;
using PolarSharp;
using PolarSharp.Models.Common;
using PolarSharp.Api;
using Xunit;

namespace PolarSharp.Tests;

public class PolarClientTests
{
    [Fact]
    public void Constructor_WithAccessToken_ShouldInitialize()
    {
        // Arrange & Act
        var client = new PolarClient("test-token");

        // Assert
        client.Should().NotBeNull();
        client.Products.Should().NotBeNull();
        client.Orders.Should().NotBeNull();
        client.Subscriptions.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullAccessToken_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new PolarClient((string)null!));
    }

    [Fact]
    public void Constructor_WithEmptyAccessToken_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new PolarClient(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceAccessToken_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new PolarClient("   "));
    }

    [Fact]
    public void Products_ShouldReturnProductsApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var productsApi = client.Products;

        // Assert
        productsApi.Should().NotBeNull();
        productsApi.Should().BeAssignableTo<ProductsApi>();
    }

    [Fact]
    public void Orders_ShouldReturnOrdersApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var ordersApi = client.Orders;

        // Assert
        ordersApi.Should().NotBeNull();
        ordersApi.Should().BeAssignableTo<OrdersApi>();
    }

    [Fact]
    public void Subscriptions_ShouldReturnSubscriptionsApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var subscriptionsApi = client.Subscriptions;

        // Assert
        subscriptionsApi.Should().NotBeNull();
        subscriptionsApi.Should().BeAssignableTo<SubscriptionsApi>();
    }

    [Fact]
    public void Customers_ShouldReturnCustomersApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var customersApi = client.Customers;

        // Assert
        customersApi.Should().NotBeNull();
        customersApi.Should().BeAssignableTo<CustomersApi>();
    }

    [Fact]
    public void Benefits_ShouldReturnBenefitsApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var benefitsApi = client.Benefits;

        // Assert
        benefitsApi.Should().NotBeNull();
        benefitsApi.Should().BeAssignableTo<BenefitsApi>();
    }

    [Fact]
    public void Checkouts_ShouldReturnCheckoutsApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var checkoutsApi = client.Checkouts;

        // Assert
        checkoutsApi.Should().NotBeNull();
        checkoutsApi.Should().BeAssignableTo<CheckoutsApi>();
    }

    [Fact]
    public void CustomerSessions_ShouldReturnCustomerSessionsApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var customerSessionsApi = client.CustomerSessions;

        // Assert
        customerSessionsApi.Should().NotBeNull();
        customerSessionsApi.Should().BeAssignableTo<CustomerSessionsApi>();
    }

    [Fact]
    public void LicenseKeys_ShouldReturnLicenseKeysApi()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var licenseKeysApi = client.LicenseKeys;

        // Assert
        licenseKeysApi.Should().NotBeNull();
        licenseKeysApi.Should().BeAssignableTo<LicenseKeysApi>();
    }

    [Fact]
    public void Create_ShouldReturnPolarClientBuilder()
    {
        // Arrange & Act
        var builder = PolarClient.Create();

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<PolarClientBuilder>();
    }
}

public class PolarClientBuilderTests
{
    [Fact]
    public void WithToken_ShouldSetAccessToken()
    {
        // Arrange & Act
        var builder = new PolarClientBuilder()
            .WithToken("test-token");

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithAccessToken_ShouldSetAccessToken()
    {
        // Arrange & Act
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token");

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithEnvironment_Sandbox_ShouldSetSandboxUrl()
    {
        // Arrange & Act
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithEnvironment(PolarEnvironment.Sandbox);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithEnvironment_Production_ShouldSetProductionUrl()
    {
        // Arrange & Act
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithEnvironment(PolarEnvironment.Production);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithBaseUrl_String_ShouldSetCustomUrl()
    {
        // Arrange & Act
        var customUrl = "https://custom-api.example.com";
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithBaseUrl(customUrl);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithBaseUrl_Uri_ShouldSetCustomUrl()
    {
        // Arrange & Act
        var customUrl = new Uri("https://custom-api.example.com");
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithBaseUrl(customUrl);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithUserAgent_ShouldSetCustomUserAgent()
    {
        // Arrange & Act
        var userAgent = "CustomAgent/1.0";
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithUserAgent(userAgent);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithTimeout_ShouldSetCustomTimeout()
    {
        // Arrange & Act
        var timeoutSeconds = 60;
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithTimeout(timeoutSeconds);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithMaxRetries_ShouldConfigureRetryPolicy()
    {
        // Arrange & Act
        var maxRetries = 5;
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithMaxRetries(maxRetries);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithoutAccessToken_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var builder = new PolarClientBuilder();
        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void WithToken_Null_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var builder = new PolarClientBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithToken(null!));
    }

    [Fact]
    public void WithAccessToken_WithValidToken_ShouldWork()
    {
        // Arrange & Act
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token");

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void WithBaseUrl_NullString_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var builder = new PolarClientBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithBaseUrl((string)null!));
    }

    [Fact]
    public void WithBaseUrl_NullUri_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var builder = new PolarClientBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithBaseUrl((Uri)null!));
    }

    [Fact]
    public void WithHttpClient_Null_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var builder = new PolarClientBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithHttpClient(null!));
    }

    [Fact]
    public void WithHttpClientFactory_Null_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var builder = new PolarClientBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithHttpClientFactory(null!));
    }

    [Fact]
    public void WithJsonOptions_ShouldSetCustomJsonOptions()
    {
        // Arrange & Act
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
        };
        var builder = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithJsonOptions(jsonOptions);

        // Assert
        var client = builder.Build();
        client.Should().NotBeNull();
    }

    [Fact]
    public void FluentConfiguration_ShouldWork()
    {
        // Arrange & Act
        var client = new PolarClientBuilder()
            .WithAccessToken("test-token")
            .WithEnvironment(PolarEnvironment.Sandbox)
            .WithTimeout(45)
            .WithMaxRetries(2)
            .WithUserAgent("TestAgent/1.0")
            .Build();

        // Assert
        client.Should().NotBeNull();
        client.Products.Should().NotBeNull();
        client.Orders.Should().NotBeNull();
    }
}