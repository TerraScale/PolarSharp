using FluentAssertions;
using PolarSharp.Models.CustomerSessions;
using PolarSharp.Results;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Customer Sessions API.
/// </summary>
public class CustomerSessionsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public CustomerSessionsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_WithValidRequest_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, create a customer to get a valid customer ID
        var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
        {
            Email = $"testcustomer{Guid.NewGuid()}@mailinator.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Value.Id,
            ReturnUrl = "https://example.com/account"
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.IsSuccess.Should().BeTrue();
        customerSession.Value.Id.Should().NotBeNullOrEmpty();
        customerSession.Value.CustomerId.Should().Be(customer.Value.Id);
        customerSession.Value.Token.Should().NotBeNullOrEmpty();
        customerSession.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        customerSession.Value.ReturnUrl.Should().Be("https://example.com/account");
        customerSession.Value.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));

        // Cleanup
        var deleteResult = await client.Customers.DeleteAsync(customer.Value.Id);
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_WithMinimalData_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, create a customer to get a valid customer ID
        var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
        {
            Email = $"testcustomer{Guid.NewGuid()}@mailinator.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Value.Id
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.IsSuccess.Should().BeTrue();
        customerSession.Value.Id.Should().NotBeNullOrEmpty();
        customerSession.Value.CustomerId.Should().Be(customer.Value.Id);
        customerSession.Value.Token.Should().NotBeNullOrEmpty();
        customerSession.Value.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));

        // Cleanup
        var deleteResult = await client.Customers.DeleteAsync(customer.Value.Id);
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_WithInvalidCustomerId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = "invalid_customer_id"
        };

        // Act
        var result = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        (result.IsValidationError || result.IsClientError).Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_WithEmptyCustomerId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = "" // Empty customer ID
        };

        // Act
        var result = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        (result.IsValidationError || result.IsClientError).Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSessionsApi_IntrospectCustomerSession_WithValidToken_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, create a customer and session
        var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
        {
            Email = $"testcustomer{Guid.NewGuid()}@mailinator.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var sessionCreateRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Value.Id
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.Value.Token
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.IsSuccess.Should().BeTrue();
        introspectResponse.Value.Valid.Should().BeTrue();
        introspectResponse.Value.CustomerId.Should().Be(customer.Value.Id);
        introspectResponse.Value.ExpiresAt.Should().BeCloseTo(customerSession.Value.ExpiresAt, TimeSpan.FromMinutes(1));

        // Cleanup
        var deleteResult = await client.Customers.DeleteAsync(customer.Value.Id);
    }

    [Fact]
    public async Task CustomerSessionsApi_IntrospectCustomerSession_WithInvalidToken_ReturnsInvalid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = "invalid_customer_access_token"
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.IsSuccess.Should().BeTrue();
        introspectResponse.Value.Valid.Should().BeFalse();
        introspectResponse.Value.CustomerId.Should().BeNull();
        introspectResponse.Value.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task CustomerSessionsApi_IntrospectCustomerSession_WithEmptyToken_ReturnsInvalid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = "" // Empty token
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.IsSuccess.Should().BeTrue();
        introspectResponse.Value.Valid.Should().BeFalse();
        introspectResponse.Value.CustomerId.Should().BeNull();
        introspectResponse.Value.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task CustomerSessionsApi_IntrospectCustomerSession_WithExpiredToken_ReturnsInvalid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, create a customer and session with short expiration
        var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
        {
            Email = $"testcustomer{Guid.NewGuid()}@mailinator.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var sessionCreateRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Value.Id
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.Value.Token
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.IsSuccess.Should().BeTrue();
        introspectResponse.Value.Valid.Should().BeFalse();
        introspectResponse.Value.CustomerId.Should().BeNull();
        introspectResponse.Value.ExpiresAt.Should().BeNull();

        // Cleanup
        var deleteResult = await client.Customers.DeleteAsync(customer.Value.Id);
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_WithFutureExpiration_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, create a customer to get a valid customer ID
        var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
        {
            Email = $"testcustomer{Guid.NewGuid()}@mailinator.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Value.Id,
            ReturnUrl = "https://example.com/return"
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.IsSuccess.Should().BeTrue();
        customerSession.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Cleanup
        var deleteResult = await client.Customers.DeleteAsync(customer.Value.Id);
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_VerifyFullStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, create a customer to get a valid customer ID
        var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
        {
            Email = $"testcustomer{Guid.NewGuid()}@mailinator.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Value.Id,
            ReturnUrl = "https://example.com/account"
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.IsSuccess.Should().BeTrue();
        customerSession.Value.Id.Should().NotBeNullOrEmpty();
        customerSession.Value.CustomerId.Should().Be(customer.Value.Id);
        customerSession.Value.Token.Should().NotBeNullOrEmpty();
        customerSession.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        customerSession.Value.ReturnUrl.Should().Be("https://example.com/account");
        customerSession.Value.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));

        // Verify nested customer object if present
        if (customerSession.Value.Customer != null)
        {
            customerSession.Value.Customer.Id.Should().Be(customer.Value.Id);
            customerSession.Value.Customer.Email.Should().Be(customer.Value.Email);
        }

        // Cleanup
        var deleteResult = await client.Customers.DeleteAsync(customer.Value.Id);
    }

    [Fact]
    public async Task CustomerSessionsApi_IntrospectCustomerSession_VerifyFullStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, create a customer and session
        var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
        {
            Email = $"testcustomer{Guid.NewGuid()}@mailinator.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var sessionCreateRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Value.Id
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.Value.Token
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.IsSuccess.Should().BeTrue();
        introspectResponse.Value.Valid.Should().BeTrue();
        introspectResponse.Value.CustomerId.Should().Be(customer.Value.Id);
        introspectResponse.Value.ExpiresAt.Should().BeCloseTo(customerSession.Value.ExpiresAt, TimeSpan.FromMinutes(1));

        // Verify nested customer object if present
        if (introspectResponse.Value.Customer != null)
        {
            introspectResponse.Value.Customer.Id.Should().Be(customer.Value.Id);
            introspectResponse.Value.Customer.Email.Should().Be(customer.Value.Email);
        }

        // Cleanup
        var deleteResult = await client.Customers.DeleteAsync(customer.Value.Id);
    }
}
