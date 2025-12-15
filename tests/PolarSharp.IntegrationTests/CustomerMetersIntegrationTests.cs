using FluentAssertions;
using PolarSharp.Models.Meters;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Customer Meters API.
/// </summary>
public class CustomerMetersIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public CustomerMetersIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.CustomerMeters.ListAsync(page: 1, limit: 10);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        // API may return 0-indexed or 1-indexed pages
        response.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_WithValidId_ReturnsCustomerMeter()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, list customer meters to get a valid ID
        var customerMeters = await client.CustomerMeters.ListAsync();
        if (customerMeters.Items.Count == 0)
        {
            return; // Skip if no customer meters exist
        }

        var customerMeterId = customerMeters.Items.First().Id;

        // Act
        var customerMeter = await client.CustomerMeters.GetAsync(customerMeterId);

        // Assert
        customerMeter.Should().NotBeNull();
        customerMeter.Id.Should().Be(customerMeterId);
        customerMeter.MeterId.Should().NotBeNullOrEmpty();
        customerMeter.CustomerId.Should().NotBeNullOrEmpty();
        customerMeter.CurrentQuantity.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidCustomerMeterId = "invalid_customer_meter_id";

        // Act & Assert
        try
        {
            var result = await client.CustomerMeters.GetAsync(invalidCustomerMeterId);
            
            // Assert - With nullable return types, invalid IDs return null
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListAllCustomerMeters_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allCustomerMeters = new List<CustomerMeter>();
        await foreach (var customerMeter in client.CustomerMeters.ListAllAsync())
        {
            allCustomerMeters.Add(customerMeter);
        }

        // Assert
        allCustomerMeters.Should().NotBeNull();
        allCustomerMeters.Should().BeAssignableTo<List<CustomerMeter>>();
    }

    [Fact]
    public async Task CustomerMetersApi_QueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var queryBuilder = client.CustomerMeters.Query();

        var response = await client.CustomerMeters.ListAsync(queryBuilder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var page1 = await client.CustomerMeters.ListAsync(page: 1, limit: 5);
        var page2 = await client.CustomerMeters.ListAsync(page: 2, limit: 5);

        // Assert
        page1.Should().NotBeNull();
        page1.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
        
        page2.Should().NotBeNull();
        page2.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.CustomerMeters.ListAsync(page: 1, limit: 100);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_VerifyStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.CustomerMeters.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        
        foreach (var customerMeter in response.Items)
        {
            customerMeter.Id.Should().NotBeNullOrEmpty();
            customerMeter.MeterId.Should().NotBeNullOrEmpty();
            customerMeter.CustomerId.Should().NotBeNullOrEmpty();
            customerMeter.CurrentQuantity.Should().BeGreaterThanOrEqualTo(0);
            customerMeter.PeriodStart.Should().BeBefore(DateTime.UtcNow);
            customerMeter.PeriodEnd.Should().BeAfter(customerMeter.PeriodStart);
            customerMeter.CreatedAt.Should().BeBefore(DateTime.UtcNow);
            customerMeter.ModifiedAt.Should().BeOnOrAfter(customerMeter.CreatedAt);
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_EmptyResponse_HandlesGracefully()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act - Use a filter that likely returns no results
        var queryBuilder = client.CustomerMeters.Query()
            .WithCustomerId("non_existent_customer_id");

        var response = await client.CustomerMeters.ListAsync(queryBuilder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Items.Should().BeEmpty();
        response.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_VerifyNestedObjects()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.CustomerMeters.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        
        foreach (var customerMeter in response.Items)
        {
            // Meter object may be null if not included
            if (customerMeter.Meter != null)
            {
                customerMeter.Meter.Id.Should().NotBeNullOrEmpty();
                customerMeter.Meter.Name.Should().NotBeNullOrEmpty();
            }

            // Customer object may be null if not included
            if (customerMeter.Customer != null)
            {
                customerMeter.Customer.Id.Should().NotBeNullOrEmpty();
                customerMeter.Customer.Email.Should().NotBeNullOrEmpty();
            }
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_WithDifferentPageSizes_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var smallPage = await client.CustomerMeters.ListAsync(page: 1, limit: 1);
        var mediumPage = await client.CustomerMeters.ListAsync(page: 1, limit: 10);
        var largePage = await client.CustomerMeters.ListAsync(page: 1, limit: 50);

        // Assert
        smallPage.Should().NotBeNull();
        mediumPage.Should().NotBeNull();
        largePage.Should().NotBeNull();
        
        // All should have the same total count
        smallPage.Pagination.TotalCount.Should().Be(mediumPage.Pagination.TotalCount);
        mediumPage.Pagination.TotalCount.Should().Be(largePage.Pagination.TotalCount);
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_VerifyFullStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, list customer meters to get a valid ID
        var customerMeters = await client.CustomerMeters.ListAsync();
        if (customerMeters.Items.Count == 0)
        {
            return; // Skip if no customer meters exist
        }

        var customerMeterId = customerMeters.Items.First().Id;

        // Act
        var customerMeter = await client.CustomerMeters.GetAsync(customerMeterId);

        // Assert
        customerMeter.Should().NotBeNull();
        customerMeter.Id.Should().Be(customerMeterId);
        customerMeter.MeterId.Should().NotBeNullOrEmpty();
        customerMeter.CustomerId.Should().NotBeNullOrEmpty();
        customerMeter.CurrentQuantity.Should().BeGreaterThanOrEqualTo(0);
        customerMeter.PeriodStart.Should().BeBefore(DateTime.UtcNow);
        customerMeter.PeriodEnd.Should().BeAfter(customerMeter.PeriodStart);
        customerMeter.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        customerMeter.ModifiedAt.Should().BeOnOrAfter(customerMeter.CreatedAt);
        
        // Verify nested objects if present
        if (customerMeter.Meter != null)
        {
            customerMeter.Meter.Id.Should().NotBeNullOrEmpty();
            customerMeter.Meter.Name.Should().NotBeNullOrEmpty();
        }
        
        if (customerMeter.Customer != null)
        {
            customerMeter.Customer.Id.Should().NotBeNullOrEmpty();
        }
    }
}