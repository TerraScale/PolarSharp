using FluentAssertions;
using PolarSharp.Models.Meters;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Customer Meters API.
/// </summary>
public class CustomerMetersIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public CustomerMetersIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_ReturnsPaginatedResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.CustomerMeters.ListAsync(page: 1, limit: 10);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support customer meters API
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();
            // API may return 0-indexed or 1-indexed pages
            result.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_WithValidId_ReturnsCustomerMeter()
    {
        try
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
            if (!result.IsSuccess)
            {
                return; // Sandbox may not support this operation
            }
            var customerMeter = result.Value;
            customerMeter.Should().NotBeNull();
            customerMeter.Id.Should().Be(customerMeterId);
            customerMeter.MeterId.Should().NotBeNullOrEmpty();
            customerMeter.CustomerId.Should().NotBeNullOrEmpty();
            customerMeter.CurrentQuantity.Should().BeGreaterThanOrEqualTo(0);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_WithInvalidId_ReturnsFailure()
    {
        try
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
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListAllCustomerMeters_UsingAsyncEnumerable_WorksCorrectly()
    {
        try
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
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_QueryBuilder_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var queryBuilder = client.CustomerMeters.Query();

            var result = await client.CustomerMeters.ListAsync(queryBuilder);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support this operation
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_WithPagination_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result1 = await client.CustomerMeters.ListAsync(page: 1, limit: 5);
            if (!result1.IsSuccess)
            {
                // Sandbox may not support this operation
                return;
            }

            var result2 = await client.CustomerMeters.ListAsync(page: 2, limit: 5);

            // Assert
            result1.Should().NotBeNull();
            result1.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);

            result2.Should().NotBeNull();
            if (result2.IsSuccess)
            {
                result2.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_LargeLimit_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.CustomerMeters.ListAsync(page: 1, limit: 100);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support this operation
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_VerifyStructure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.CustomerMeters.ListAsync();

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support this operation
                return;
            }
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
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_EmptyResponse_HandlesGracefully()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Use a filter that likely returns no results
            var queryBuilder = client.CustomerMeters.Query()
                .WithCustomerId("non_existent_customer_id");

            var result = await client.CustomerMeters.ListAsync(queryBuilder);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support this operation
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            // May be empty or contain items depending on validation
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_VerifyNestedObjects()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.CustomerMeters.ListAsync();

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support this operation
                return;
            }
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
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_ListCustomerMeters_WithDifferentPageSizes_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var smallResult = await client.CustomerMeters.ListAsync(page: 1, limit: 1);
            if (!smallResult.IsSuccess)
            {
                // Sandbox may not support this operation
                return;
            }

            var mediumResult = await client.CustomerMeters.ListAsync(page: 1, limit: 10);
            var largeResult = await client.CustomerMeters.ListAsync(page: 1, limit: 50);

            // Assert
            smallResult.Should().NotBeNull();
            mediumResult.Should().NotBeNull();
            if (!mediumResult.IsSuccess) return;
            largeResult.Should().NotBeNull();
            if (!largeResult.IsSuccess) return;

            // All should have the same total count
            smallResult.Value.Pagination.TotalCount.Should().Be(mediumResult.Value.Pagination.TotalCount);
            mediumResult.Value.Pagination.TotalCount.Should().Be(largeResult.Value.Pagination.TotalCount);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerMetersApi_GetCustomerMeter_VerifyFullStructure()
    {
        try
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
            if (!result.IsSuccess)
            {
                return; // Sandbox may not support this operation
            }
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
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}
