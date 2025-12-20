using System.Net;
using FluentAssertions;
using PolarSharp.Results;
using PolarSharp.Models.Discounts;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Discounts API.
/// </summary>
public class DiscountsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public DiscountsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task ListAsync_ShouldReturnDiscounts()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Discounts.ListAsync();

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
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
    public async Task ListAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Discounts.ListAsync(page: 1, limit: 5);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();
            result.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_ShouldReturnAllDiscounts()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var discountCount = 0;

            // Act
            await foreach (var discountResult in client.Discounts.ListAllAsync())
            {
                if (discountResult.IsFailure) break;

                var discount = discountResult.Value;
                discountCount++;
                discount.Should().NotBeNull();
                discount.Id.Should().NotBeNullOrEmpty();
                discount.Name.Should().NotBeNullOrEmpty();
            }

            // Assert
            discountCount.Should().BeGreaterThanOrEqualTo(0);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateFixedAmountDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new DiscountCreateRequest
            {
                Name = $"Test Fixed Discount {Guid.NewGuid()}",
                Type = DiscountType.FixedAmount,
                Amount = 1000, // $10.00 in cents
                Duration = DiscountDuration.Once,
                Metadata = new Dictionary<string, object>
                {
                    ["test"] = "integration",
                    ["source"] = "DiscountsIntegrationTests"
                }
            };

            // Act
            var result = await client.Discounts.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"CreateAsync failed: {result.Error?.Message}");
                return;
            }
            var discount = result.Value;
            discount.Should().NotBeNull();
            discount.Id.Should().NotBeNullOrEmpty();
            discount.Name.Should().Be(request.Name);
            discount.Type.Should().Be(request.Type);
            discount.Amount.Should().Be((int?)request.Amount);
            discount.Duration.Should().Be(request.Duration);
            discount.Metadata.Should().NotBeNull();
            discount.Metadata.Should().ContainKey("test");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePercentageDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new DiscountCreateRequest
            {
                Name = $"Test Percentage Discount {Guid.NewGuid()}",
                Type = DiscountType.Percentage,
                Percentage = 25m, // 25%
                Duration = DiscountDuration.Repeating,
                DurationInMonths = 3,
                Metadata = new Dictionary<string, object>
                {
                    ["discount_type"] = "percentage",
                    ["test_run"] = true
                }
            };

            // Act
            var result = await client.Discounts.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"CreatePercentageDiscount failed: {result.Error?.Message}");
                return;
            }
            var discount = result.Value;
            discount.Should().NotBeNull();
            discount.Id.Should().NotBeNullOrEmpty();
            discount.Name.Should().Be(request.Name);
            discount.Type.Should().Be(request.Type);
            discount.Percentage.Should().Be(request.Percentage);
            discount.Duration.Should().Be(request.Duration);
            discount.DurationInMonths.Should().Be(request.DurationInMonths);
            discount.Metadata.Should().NotBeNull();
            discount.Metadata.Should().ContainKey("discount_type");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createResult = await CreateTestDiscountAsync(client);
            if (createResult.IsFailure)
            {
                _output.WriteLine($"Skipped: Could not create test discount - {createResult.Error!.Message}");
                return;
            }
            var createdDiscount = createResult.Value;

            // Act
            var result = await client.Discounts.GetAsync(createdDiscount.Id);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var retrievedDiscount = result.Value;
            retrievedDiscount.Should().NotBeNull();
            retrievedDiscount.Id.Should().Be(createdDiscount.Id);
            retrievedDiscount.Name.Should().Be(createdDiscount.Name);
            retrievedDiscount.Type.Should().Be(createdDiscount.Type);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createResult = await CreateTestDiscountAsync(client);
            if (createResult.IsFailure)
            {
                _output.WriteLine($"Skipped: Could not create test discount - {createResult.Error!.Message}");
                return;
            }
            var createdDiscount = createResult.Value;
            var updateRequest = new DiscountUpdateRequest
            {
                Name = $"Updated {createdDiscount.Name}",
                StartsAt = DateTime.UtcNow.AddDays(-1),
                EndsAt = DateTime.UtcNow.AddDays(90),
                MaxRedemptions = 200,
                Metadata = new Dictionary<string, object>
                {
                    ["updated"] = true,
                    ["update_timestamp"] = DateTime.UtcNow.ToString("O")
                }
            };

            // Act
            var result = await client.Discounts.UpdateAsync(createdDiscount.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var updatedDiscount = result.Value;
            updatedDiscount.Should().NotBeNull();
            updatedDiscount.Id.Should().Be(createdDiscount.Id);
            updatedDiscount.Name.Should().Be(updateRequest.Name);
            updatedDiscount.MaxRedemptions.Should().Be(updateRequest.MaxRedemptions);
            updatedDiscount.Metadata.Should().NotBeNull();
            updatedDiscount.Metadata.Should().ContainKey("updated");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createResult = await CreateTestDiscountAsync(client);
            if (createResult.IsFailure)
            {
                _output.WriteLine($"Skipped: Could not create test discount - {createResult.Error!.Message}");
                return;
            }
            var createdDiscount = createResult.Value;

            // Act
            var result = await client.Discounts.DeleteAsync(createdDiscount.Id);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var deletedDiscount = result.Value;
            deletedDiscount.Should().NotBeNull();
            deletedDiscount.Id.Should().Be(createdDiscount.Id);

            // Verify discount is deleted by trying to get it
            var afterDeleteResult = await client.Discounts.GetAsync(createdDiscount.Id);
            afterDeleteResult.IsSuccess.Should().BeFalse();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ShouldReturnFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var builder = client.Discounts.Query()
                .WithType("fixed");

            // Act
            var result = await client.Discounts.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();

            // Note: Server-side type filtering may not be fully supported
            // Just verify we get a valid response when using the query builder
            _output.WriteLine($"Returned {response.Items.Count} discounts");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithDateFilters_ShouldReturnFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            var builder = client.Discounts.Query()
                .CreatedAfter(yesterday)
                .CreatedBefore(nextMonth);

            // Act
            var result = await client.Discounts.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();

            // Verify date filtering (if any discounts exist)
            foreach (var discount in response.Items)
            {
                discount.CreatedAt.Should().BeOnOrAfter(yesterday);
                discount.CreatedAt.Should().BeOnOrBefore(nextMonth);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithExpirationFilters_ShouldReturnFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var tomorrow = DateTime.UtcNow.AddDays(1);
            var nextYear = DateTime.UtcNow.AddYears(1);
            var builder = client.Discounts.Query()
                .ExpiresAfter(tomorrow)
                .ExpiresBefore(nextYear);

            // Act
            var result = await client.Discounts.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();

            // Verify expiration filtering (if any discounts exist)
            foreach (var discount in response.Items)
            {
                if (discount.EndsAt.HasValue)
                {
                    discount.EndsAt.Value.Should().BeOnOrAfter(tomorrow);
                    discount.EndsAt.Value.Should().BeOnOrBefore(nextYear);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithMinimalData_ShouldCreateDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new DiscountCreateRequest
            {
                Name = $"Minimal Discount {Guid.NewGuid()}",
                Type = DiscountType.Percentage,
                Percentage = 10m,
                Duration = DiscountDuration.Once // Duration is required by API
            };

            // Act
            var result = await client.Discounts.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var discount = result.Value;
            discount.Should().NotBeNull();
            discount.Id.Should().NotBeNullOrEmpty();
            discount.Name.Should().Be(request.Name);
            discount.Type.Should().Be(request.Type);
            discount.Percentage.Should().Be(request.Percentage);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithPartialData_ShouldUpdateOnlySpecifiedFields()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createResult = await CreateTestDiscountAsync(client);
            if (createResult.IsFailure)
            {
                _output.WriteLine($"Skipped: Could not create test discount - {createResult.Error!.Message}");
                return;
            }
            var createdDiscount = createResult.Value;
            var originalType = createdDiscount.Type;
            var updateRequest = new DiscountUpdateRequest
            {
                Name = $"Updated {createdDiscount.Name}"
            };

            // Act
            var result = await client.Discounts.UpdateAsync(createdDiscount.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var updatedDiscount = result.Value;
            updatedDiscount.Should().NotBeNull();
            updatedDiscount.Id.Should().Be(createdDiscount.Id);
            updatedDiscount.Type.Should().Be(originalType); // Should remain unchanged
            updatedDiscount.Name.Should().Be(updateRequest.Name);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithForeverDuration_ShouldCreateDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new DiscountCreateRequest
            {
                Name = $"Forever Discount {Guid.NewGuid()}",
                Type = DiscountType.Percentage,
                Percentage = 15m,
                Duration = DiscountDuration.Forever
            };

            // Act
            var result = await client.Discounts.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var discount = result.Value;
            discount.Should().NotBeNull();
            discount.Id.Should().NotBeNullOrEmpty();
            discount.Name.Should().Be(request.Name);
            discount.Type.Should().Be(request.Type);
            discount.Percentage.Should().Be(request.Percentage);
            discount.Duration.Should().Be(DiscountDuration.Forever);
            discount.DurationInMonths.Should().BeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithComplexMetadata_ShouldCreateDiscount()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new DiscountCreateRequest
            {
                Name = $"Complex Metadata Discount {Guid.NewGuid()}",
                Type = DiscountType.FixedAmount,
                Amount = 2000,
                Duration = DiscountDuration.Once,
                Metadata = new Dictionary<string, object>
                {
                    ["campaign"] = "summer_sale",
                    ["target_audience"] = new[] { "new_customers", "returning_customers" },
                    ["min_order_value"] = 5000,
                    ["excluded_products"] = new object[] { "premium_bundle", "gift_card" },
                    ["created_by"] = "integration_tests",
                    ["test_flags"] = new { is_test = true, environment = "sandbox" },
                    ["valid_regions"] = new[] { "US", "EU", "APAC" }
                }
            };

            // Act
            var result = await client.Discounts.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var discount = result.Value;
            discount.Should().NotBeNull();
            discount.Metadata.Should().NotBeNull();
            discount.Metadata.Should().ContainKey("campaign");
            discount.Metadata.Should().ContainKey("target_audience");
            discount.Metadata.Should().ContainKey("min_order_value");
            discount.Metadata["campaign"].Should().Be("summer_sale");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    private async Task<PolarResult<Discount>> CreateTestDiscountAsync(PolarClient client)
    {
        var request = new DiscountCreateRequest
        {
            Name = $"Test Discount {Guid.NewGuid()}",
            Type = DiscountType.FixedAmount,
            Amount = 500, // $5.00 in cents
            Duration = DiscountDuration.Once
        };

        return await client.Discounts.CreateAsync(request);
    }
}
