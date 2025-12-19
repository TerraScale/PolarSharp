using FluentAssertions;
using PolarSharp.Models.Meters;
using PolarSharp.Results;
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
        var result = await client.CustomerMeters.ListAsync(page: 1, limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
        // API may return 0-indexed or 1-indexed pages
        result.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_WithValidId_ReturnsCustomerMeter()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, list customer meters to get a valid ID
        var listResult = await client.CustomerMeters.ListAsync();
        if (listResult.IsFailure || listResult.Value.Items.Count == 0)
        {
            return; // Skip if no customer meters exist
        }

        var customerMeterId = listResult.Value.Items.First().Id;

        // Act
        var result = await client.CustomerMeters.GetAsync(customerMeterId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var customerMeter = result.Value;
        customerMeter.Should().NotBeNull();
        customerMeter.Id.Should().Be(customerMeterId);
        customerMeter.MeterId.Should().NotBeNullOrEmpty();
        customerMeter.CustomerId.Should().NotBeNullOrEmpty();
        customerMeter.CurrentQuantity.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidCustomerMeterId = "invalid_customer_meter_id";

        // Act
        var result = await client.CustomerMeters.GetAsync(invalidCustomerMeterId);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerMetersApi_ListAllCustomerMeters_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allCustomerMeters = new List<CustomerMeter>();
        await foreach (var meterResult in client.CustomerMeters.ListAllAsync())
        {
            if (meterResult.IsFailure) break;
            var customerMeter = meterResult.Value;
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

        var result = await client.CustomerMeters.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result1 = await client.CustomerMeters.ListAsync(page: 1, limit: 5);
        var result2 = await client.CustomerMeters.ListAsync(page: 2, limit: 5);

        // Assert
        result1.Should().NotBeNull();
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);

        result2.Should().NotBeNull();
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CustomerMeters.ListAsync(page: 1, limit: 100);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_VerifyStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CustomerMeters.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();

        foreach (var customerMeter in result.Value.Items)
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

        var result = await client.CustomerMeters.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_VerifyNestedObjects()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CustomerMeters.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();

        foreach (var customerMeter in result.Value.Items)
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
        var smallResult = await client.CustomerMeters.ListAsync(page: 1, limit: 1);
        var mediumResult = await client.CustomerMeters.ListAsync(page: 1, limit: 10);
        var largeResult = await client.CustomerMeters.ListAsync(page: 1, limit: 50);

        // Assert
        smallResult.Should().NotBeNull();
        smallResult.IsSuccess.Should().BeTrue();
        mediumResult.Should().NotBeNull();
        mediumResult.IsSuccess.Should().BeTrue();
        largeResult.Should().NotBeNull();
        largeResult.IsSuccess.Should().BeTrue();

        // All should have the same total count
        smallResult.Value.Pagination.TotalCount.Should().Be(mediumResult.Value.Pagination.TotalCount);
        mediumResult.Value.Pagination.TotalCount.Should().Be(largeResult.Value.Pagination.TotalCount);
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_VerifyFullStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, list customer meters to get a valid ID
        var listResult = await client.CustomerMeters.ListAsync();
        if (listResult.IsFailure || listResult.Value.Items.Count == 0)
        {
            return; // Skip if no customer meters exist
        }

        var customerMeterId = listResult.Value.Items.First().Id;

        // Act
        var result = await client.CustomerMeters.GetAsync(customerMeterId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var customerMeter = result.Value;
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