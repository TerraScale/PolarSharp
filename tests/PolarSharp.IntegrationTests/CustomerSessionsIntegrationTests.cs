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
            Email = $"testcustomer{Guid.NewGuid()}@example.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Id,
            CustomerAccessTokenExpiresAt = DateTime.UtcNow.AddHours(24),
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["environment"] = "integration"
            }
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.Id.Should().NotBeNullOrEmpty();
        customerSession.CustomerId.Should().Be(customer.Id);
        customerSession.CustomerAccessToken.Should().NotBeNullOrEmpty();
        customerSession.CustomerAccessTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
        customerSession.Metadata.Should().NotBeNull();
        customerSession.Metadata.Should().ContainKey("test");
        customerSession.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        customerSession.UpdatedAt.Should().BeOnOrAfter(customerSession.CreatedAt);

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
            Email = $"testcustomer{Guid.NewGuid()}@example.com",
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
        customerSession.CustomerAccessToken.Should().NotBeNullOrEmpty();
        customerSession.Metadata.Should().BeNull();
        customerSession.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        customerSession.UpdatedAt.Should().BeOnOrAfter(customerSession.CreatedAt);

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
            Email = $"testcustomer{Guid.NewGuid()}@example.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var sessionCreateRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Id,
            CustomerAccessTokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.CustomerAccessToken
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.Valid.Should().BeTrue();
        introspectResponse.CustomerId.Should().Be(customer.Id);
        introspectResponse.ExpiresAt.Should().BeCloseTo(customerSession.CustomerAccessTokenExpiresAt, TimeSpan.FromMinutes(1));

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
            Email = $"testcustomer{Guid.NewGuid()}@example.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var sessionCreateRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Id,
            CustomerAccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(-1) // Already expired
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.CustomerAccessToken
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
            Email = $"testcustomer{Guid.NewGuid()}@example.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var futureExpiration = DateTime.UtcNow.AddDays(30);

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Id,
            CustomerAccessTokenExpiresAt = futureExpiration
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.CustomerAccessTokenExpiresAt.Should().BeCloseTo(futureExpiration, TimeSpan.FromMinutes(1));

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
            Email = $"testcustomer{Guid.NewGuid()}@example.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var createRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Id,
            CustomerAccessTokenExpiresAt = DateTime.UtcNow.AddHours(12),
            Metadata = new Dictionary<string, object>
            {
                ["source"] = "integration_test",
                ["version"] = "1.0"
            }
        };

        // Act
        var customerSession = await client.CustomerSessions.CreateAsync(createRequest);

        // Assert
        customerSession.Should().NotBeNull();
        customerSession.Id.Should().NotBeNullOrEmpty();
        customerSession.CustomerId.Should().Be(customer.Id);
        customerSession.CustomerAccessToken.Should().NotBeNullOrEmpty();
        customerSession.CustomerAccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        customerSession.Metadata.Should().NotBeNull();
        customerSession.Metadata.Should().ContainKey("source");
        customerSession.Metadata.Should().ContainKey("version");
        customerSession.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        customerSession.UpdatedAt.Should().BeOnOrAfter(customerSession.CreatedAt);
        
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
            Email = $"testcustomer{Guid.NewGuid()}@example.com",
            Name = "Test Customer"
        };

        var customer = await client.Customers.CreateAsync(customerRequest);

        var sessionCreateRequest = new CustomerSessionCreateRequest
        {
            CustomerId = customer.Id,
            CustomerAccessTokenExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        var customerSession = await client.CustomerSessions.CreateAsync(sessionCreateRequest);

        var introspectRequest = new CustomerSessionIntrospectRequest
        {
            CustomerAccessToken = customerSession.CustomerAccessToken
        };

        // Act
        var introspectResponse = await client.CustomerSessions.IntrospectAsync(introspectRequest);

        // Assert
        introspectResponse.Should().NotBeNull();
        introspectResponse.Valid.Should().BeTrue();
        introspectResponse.CustomerId.Should().Be(customer.Id);
        introspectResponse.ExpiresAt.Should().BeCloseTo(customerSession.CustomerAccessTokenExpiresAt, TimeSpan.FromMinutes(1));
        
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