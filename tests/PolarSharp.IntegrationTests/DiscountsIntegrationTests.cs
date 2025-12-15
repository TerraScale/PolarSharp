using System.Net;
using FluentAssertions;
using PolarSharp.Exceptions;
using PolarSharp.Models.Discounts;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Discounts API.
/// </summary>
public class DiscountsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public DiscountsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListAsync_ShouldReturnDiscounts()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Discounts.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Discounts.ListAsync(page: 1, limit: 5);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListAllAsync_ShouldReturnAllDiscounts()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var discountCount = 0;

        // Act
        await foreach (var discount in client.Discounts.ListAllAsync())
        {
            discountCount++;
            discount.Should().NotBeNull();
            discount.Id.Should().NotBeNullOrEmpty();
            discount.Name.Should().NotBeNullOrEmpty();
        }

        // Assert
        discountCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateFixedAmountDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DiscountCreateRequest
        {
            Name = $"Test Fixed Discount {Guid.NewGuid()}",
            Description = "Test fixed amount discount created via integration test",
            Type = DiscountType.FixedAmount,
            Amount = 1000, // $10.00 in cents
            Currency = "USD",
            Active = true,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(30),
            MaxRedemptions = 100,
            Duration = DiscountDuration.Once,
            Metadata = new Dictionary<string, object>
            {
                ["test"] = "integration",
                ["source"] = "DiscountsIntegrationTests"
            }
        };

        // Act
        var discount = await client.Discounts.CreateAsync(request);

        // Assert
        discount.Should().NotBeNull();
        discount.Id.Should().NotBeNullOrEmpty();
        discount.Name.Should().Be(request.Name);
        discount.Description.Should().Be(request.Description);
        discount.Type.Should().Be(request.Type);
        discount.Amount.Should().Be((int?)request.Amount);
        discount.Currency.Should().Be(request.Currency);
        discount.Active.Should().Be(request.Active);
        discount.MaxRedemptions.Should().Be(request.MaxRedemptions);
        discount.Duration.Should().Be(request.Duration);
        discount.Metadata.Should().NotBeNull();
        discount.Metadata.Should().ContainKey("test");
        discount.Metadata["test"].Should().Be("integration");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePercentageDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DiscountCreateRequest
        {
            Name = $"Test Percentage Discount {Guid.NewGuid()}",
            Description = "Test percentage discount created via integration test",
            Type = DiscountType.Percentage,
            Percentage = 25.5m,
            Active = true,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(60),
            Duration = DiscountDuration.Repeating,
            DurationInMonths = 3,
            Metadata = new Dictionary<string, object>
            {
                ["discount_type"] = "percentage",
                ["test_run"] = true
            }
        };

        // Act
        var discount = await client.Discounts.CreateAsync(request);

        // Assert
        discount.Should().NotBeNull();
        discount.Id.Should().NotBeNullOrEmpty();
        discount.Name.Should().Be(request.Name);
        discount.Description.Should().Be(request.Description);
        discount.Type.Should().Be(request.Type);
        discount.Percentage.Should().Be(request.Percentage);
        discount.Active.Should().Be(request.Active);
        discount.Duration.Should().Be(request.Duration);
        discount.DurationInMonths.Should().Be(request.DurationInMonths);
        discount.Metadata.Should().NotBeNull();
        discount.Metadata.Should().ContainKey("discount_type");
        discount.Metadata["discount_type"].Should().Be("percentage");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdDiscount = await CreateTestDiscountAsync(client);

        // Act
        var retrievedDiscount = await client.Discounts.GetAsync(createdDiscount.Id);

        // Assert
        retrievedDiscount.Should().NotBeNull();
        retrievedDiscount.Id.Should().Be(createdDiscount.Id);
        retrievedDiscount.Name.Should().Be(createdDiscount.Name);
        retrievedDiscount.Type.Should().Be(createdDiscount.Type);
        retrievedDiscount.Active.Should().Be(createdDiscount.Active);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdDiscount = await CreateTestDiscountAsync(client);
        var updateRequest = new DiscountUpdateRequest
        {
            Name = $"Updated {createdDiscount.Name}",
            Description = "Updated discount description",
            Active = false,
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
        var updatedDiscount = await client.Discounts.UpdateAsync(createdDiscount.Id, updateRequest);

        // Assert
        updatedDiscount.Should().NotBeNull();
        updatedDiscount.Id.Should().Be(createdDiscount.Id);
        updatedDiscount.Name.Should().Be(updateRequest.Name);
        updatedDiscount.Description.Should().Be(updateRequest.Description);
        updatedDiscount.Active.Should().Be(updateRequest.Active.Value);
        updatedDiscount.MaxRedemptions.Should().Be(updateRequest.MaxRedemptions);
        updatedDiscount.Metadata.Should().NotBeNull();
        updatedDiscount.Metadata.Should().ContainKey("updated");
        updatedDiscount.Metadata["updated"].Should().Be(true);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdDiscount = await CreateTestDiscountAsync(client);

        // Act
        var deletedDiscount = await client.Discounts.DeleteAsync(createdDiscount.Id);

        // Assert
        deletedDiscount.Should().NotBeNull();
        deletedDiscount!.Id.Should().Be(createdDiscount.Id);
        
        // Verify discount is deleted by trying to get it (returns null for deleted items)
        var afterDelete = await client.Discounts.GetAsync(createdDiscount.Id);
        afterDelete.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var builder = client.Discounts.Query()
            .WithActive(true)
            .WithType("fixed_amount");

        // Act
        var response = await client.Discounts.ListAsync(builder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        
        // Verify filtering (if any discounts exist)
        foreach (var discount in response.Items)
        {
            discount.Active.Should().BeTrue();
            discount.Type.Should().Be(DiscountType.FixedAmount);
        }
    }

    [Fact]
    public async Task ListAsync_WithDateFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var nextMonth = DateTime.UtcNow.AddMonths(1);
        var builder = client.Discounts.Query()
            .CreatedAfter(yesterday)
            .CreatedBefore(nextMonth);

        // Act
        var response = await client.Discounts.ListAsync(builder);

        // Assert
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

    [Fact]
    public async Task ListAsync_WithExpirationFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var nextYear = DateTime.UtcNow.AddYears(1);
        var builder = client.Discounts.Query()
            .ExpiresAfter(tomorrow)
            .ExpiresBefore(nextYear);

        // Act
        var response = await client.Discounts.ListAsync(builder);

        // Assert
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

    [Fact]
    public async Task CreateAsync_WithMinimalData_ShouldCreateDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DiscountCreateRequest
        {
            Name = $"Minimal Discount {Guid.NewGuid()}",
            Type = DiscountType.Percentage,
            Percentage = 10m
        };

        // Act
        var discount = await client.Discounts.CreateAsync(request);

        // Assert
        discount.Should().NotBeNull();
        discount.Id.Should().NotBeNullOrEmpty();
        discount.Name.Should().Be(request.Name);
        discount.Type.Should().Be(request.Type);
        discount.Percentage.Should().Be(request.Percentage);
        discount.Description.Should().BeNull();
        discount.Active.Should().BeTrue(); // Default value
        discount.Duration.Should().BeNull(); // Not specified
    }

    [Fact]
    public async Task UpdateAsync_WithPartialData_ShouldUpdateOnlySpecifiedFields()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdDiscount = await CreateTestDiscountAsync(client);
        var originalType = createdDiscount.Type;
        var updateRequest = new DiscountUpdateRequest
        {
            Description = "Updated description only"
        };

        // Act
        var updatedDiscount = await client.Discounts.UpdateAsync(createdDiscount.Id, updateRequest);

        // Assert
        updatedDiscount.Should().NotBeNull();
        updatedDiscount.Id.Should().Be(createdDiscount.Id);
        updatedDiscount.Type.Should().Be(originalType); // Should remain unchanged
        updatedDiscount.Description.Should().Be(updateRequest.Description);
    }

    [Fact]
    public async Task CreateAsync_WithForeverDuration_ShouldCreateDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DiscountCreateRequest
        {
            Name = $"Forever Discount {Guid.NewGuid()}",
            Type = DiscountType.Percentage,
            Percentage = 15m,
            Duration = DiscountDuration.Forever,
            Active = true
        };

        // Act
        var discount = await client.Discounts.CreateAsync(request);

        // Assert
        discount.Should().NotBeNull();
        discount.Id.Should().NotBeNullOrEmpty();
        discount.Name.Should().Be(request.Name);
        discount.Type.Should().Be(request.Type);
        discount.Percentage.Should().Be(request.Percentage);
        discount.Duration.Should().Be(DiscountDuration.Forever);
        discount.DurationInMonths.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithComplexMetadata_ShouldCreateDiscount()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DiscountCreateRequest
        {
            Name = $"Complex Metadata Discount {Guid.NewGuid()}",
            Type = DiscountType.FixedAmount,
            Amount = 2000,
            Currency = "EUR",
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
        var discount = await client.Discounts.CreateAsync(request);

        // Assert
        discount.Should().NotBeNull();
        discount.Metadata.Should().NotBeNull();
        discount.Metadata.Should().ContainKey("campaign");
        discount.Metadata.Should().ContainKey("target_audience");
        discount.Metadata.Should().ContainKey("min_order_value");
        discount.Metadata["campaign"].Should().Be("summer_sale");
    }

    private async Task<Discount> CreateTestDiscountAsync(PolarClient client)
    {
        var request = new DiscountCreateRequest
        {
            Name = $"Test Discount {Guid.NewGuid()}",
            Description = "Test discount for integration tests",
            Type = DiscountType.FixedAmount,
            Amount = 500, // $5.00 in cents
            Currency = "USD",
            Active = true,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(30),
            MaxRedemptions = 50,
            Duration = DiscountDuration.Once,
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["created_at"] = DateTime.UtcNow.ToString("O")
            }
        };

        return await client.Discounts.CreateAsync(request);
    }
}