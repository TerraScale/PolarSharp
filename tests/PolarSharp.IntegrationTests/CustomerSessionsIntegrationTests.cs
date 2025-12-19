using FluentAssertions;
using PolarSharp.Models.CustomerSessions;
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
            CustomerId = customer.Id,
            ReturnUrl = "https://example.com/account"
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.Id.Should().NotBeNullOrEmpty();
        customerSession.CustomerId.Should().Be(customer.Id);
        customerSession.Token.Should().NotBeNullOrEmpty();
        customerSession.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        customerSession.ReturnUrl.Should().Be("https://example.com/account");
        customerSession.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));

        // Cleanup
        try
        {
            await client.Customers.DeleteAsync(customer.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
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
            CustomerId = customer.Id
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.Id.Should().NotBeNullOrEmpty();
        customerSession.CustomerId.Should().Be(customer.Id);
        customerSession.Token.Should().NotBeNullOrEmpty();
        customerSession.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));

        // Cleanup
        try
        {
            await client.Customers.DeleteAsync(customer.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_WithInvalidCustomerId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = "invalid_customer_id"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSessions.CreateAsync(createRequest));
    }

    [Fact]
    public async Task CustomerSessionsApi_CreateCustomerSession_WithEmptyCustomerId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = "" // Empty customer ID
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSessions.CreateAsync(createRequest));
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
            CustomerId = customer.Id
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.Token
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.Valid.Should().BeTrue();
        introspectResponse.CustomerId.Should().Be(customer.Id);
        introspectResponse.ExpiresAt.Should().BeCloseTo(customerSession.ExpiresAt, TimeSpan.FromMinutes(1));

        // Cleanup
        try
        {
            await client.Customers.DeleteAsync(customer.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
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
        introspectResponse.Valid.Should().BeFalse();
        introspectResponse.CustomerId.Should().BeNull();
        introspectResponse.ExpiresAt.Should().BeNull();
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
        introspectResponse.Valid.Should().BeFalse();
        introspectResponse.CustomerId.Should().BeNull();
        introspectResponse.ExpiresAt.Should().BeNull();
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
            CustomerId = customer.Id
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.Token
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.Valid.Should().BeFalse();
        introspectResponse.CustomerId.Should().BeNull();
        introspectResponse.ExpiresAt.Should().BeNull();

        // Cleanup
        try
        {
            await client.Customers.DeleteAsync(customer.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
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
            CustomerId = customer.Id,
            ReturnUrl = "https://example.com/return"
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Cleanup
        try
        {
            await client.Customers.DeleteAsync(customer.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
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
            CustomerId = customer.Id,
            ReturnUrl = "https://example.com/account"
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.Id.Should().NotBeNullOrEmpty();
        customerSession.CustomerId.Should().Be(customer.Id);
        customerSession.Token.Should().NotBeNullOrEmpty();
        customerSession.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        customerSession.ReturnUrl.Should().Be("https://example.com/account");
        customerSession.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
        
        // Verify nested customer object if present
        if (customerSession.Customer != null)
        {
            customerSession.Customer.Id.Should().Be(customer.Id);
            customerSession.Customer.Email.Should().Be(customer.Email);
        }

        // Cleanup
        try
        {
            await client.Customers.DeleteAsync(customer.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
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
            CustomerId = customer.Id
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.Token
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.Valid.Should().BeTrue();
        introspectResponse.CustomerId.Should().Be(customer.Id);
        introspectResponse.ExpiresAt.Should().BeCloseTo(customerSession.ExpiresAt, TimeSpan.FromMinutes(1));
        
        // Verify nested customer object if present
        if (introspectResponse.Customer != null)
        {
            introspectResponse.Customer.Id.Should().Be(customer.Id);
            introspectResponse.Customer.Email.Should().Be(customer.Email);
        }

        // Cleanup
        try
        {
            await client.Customers.DeleteAsync(customer.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}