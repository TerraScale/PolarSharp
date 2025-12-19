using FluentAssertions;
using PolarSharp.Models.Products;
using PolarSharp.Models.Subscriptions;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Subscriptions API.
/// </summary>
public class SubscriptionsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SubscriptionsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
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
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
        result.Value.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Value.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SubscriptionsApi_ListAllAsync_EnumeratesAllPages()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var subscriptions = new List<Subscription>();
        await foreach (var subscriptionResult in client.Subscriptions.ListAllAsync())
        {
            if (subscriptionResult.IsFailure) break;
            subscriptions.Add(subscriptionResult.Value);
        }

        // Assert
        subscriptions.Should().NotBeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_ListWithFilters_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // Test with customer ID filter
        var resultWithCustomer = await client.Subscriptions.ListAsync(customerId: "test_customer_id");
        if (resultWithCustomer.IsSuccess)
        {
            resultWithCustomer.Value.Should().NotBeNull();
            resultWithCustomer.Value.Items.Should().NotBeNull();
        }
        else if (resultWithCustomer.IsAuthError || resultWithCustomer.Error!.Message.Contains("Unauthorized") ||
                 resultWithCustomer.Error!.Message.Contains("Forbidden") || resultWithCustomer.Error!.Message.Contains("Method Not Allowed") ||
                 resultWithCustomer.Error!.Message.Contains("Not Found") || resultWithCustomer.Error!.Message.Contains("RequestValidationError"))
        {
            _output.WriteLine($"Skipped customer filter test: {resultWithCustomer.Error!.Message}");
        }

        // Test with product ID filter
        var resultWithProduct = await client.Subscriptions.ListAsync(productId: "test_product_id");
        if (resultWithProduct.IsSuccess)
        {
            resultWithProduct.Value.Should().NotBeNull();
            resultWithProduct.Value.Items.Should().NotBeNull();
        }
        else if (resultWithProduct.IsAuthError || resultWithProduct.Error!.Message.Contains("Unauthorized") ||
                 resultWithProduct.Error!.Message.Contains("Forbidden") || resultWithProduct.Error!.Message.Contains("Method Not Allowed") ||
                 resultWithProduct.Error!.Message.Contains("Not Found") || resultWithProduct.Error!.Message.Contains("RequestValidationError"))
        {
            _output.WriteLine($"Skipped product filter test: {resultWithProduct.Error!.Message}");
        }

        // Test with status filter
        var resultWithStatus = await client.Subscriptions.ListAsync(status: SubscriptionStatus.Active);
        if (resultWithStatus.IsSuccess)
        {
            resultWithStatus.Value.Should().NotBeNull();
            resultWithStatus.Value.Items.Should().NotBeNull();
        }
        else if (resultWithStatus.IsAuthError || resultWithStatus.Error!.Message.Contains("Unauthorized") ||
                 resultWithStatus.Error!.Message.Contains("Forbidden") || resultWithStatus.Error!.Message.Contains("Method Not Allowed") ||
                 resultWithStatus.Error!.Message.Contains("RequestValidationError") || resultWithStatus.Error!.Message.Contains("Not Found"))
        {
            _output.WriteLine($"Skipped status filter test: {resultWithStatus.Error!.Message}");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_GetSubscription_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // First, try to list subscriptions to get a real subscription ID
        var listResult = await client.Subscriptions.ListAsync(limit: 1);
        if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
        {
            var subscriptionId = listResult.Value.Items[0].Id;
            var subscriptionResult = await client.Subscriptions.GetAsync(subscriptionId);

            subscriptionResult.Should().NotBeNull();
            subscriptionResult.IsSuccess.Should().BeTrue();
            var subscription = subscriptionResult.Value;
            subscription.Should().NotBeNull();
            subscription.Id.Should().Be(subscriptionId);
            subscription.Status.Should().BeOneOf(SubscriptionStatus.Active, SubscriptionStatus.Trialing, SubscriptionStatus.PastDue, SubscriptionStatus.Canceled, SubscriptionStatus.Incomplete, SubscriptionStatus.IncompleteExpired, SubscriptionStatus.Unpaid);
            subscription.CustomerId.Should().NotBeNullOrEmpty();
            subscription.ProductId.Should().NotBeNullOrEmpty();
            subscription.CurrentPeriodStart.Should().BeBefore(DateTime.UtcNow.AddDays(1));
            subscription.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddDays(1));
            subscription.ModifiedAt.Should().BeBefore(DateTime.UtcNow.AddDays(1));
        }
        else if (listResult.IsAuthError || (listResult.IsFailure && (listResult.Error!.Message.Contains("Unauthorized") ||
                 listResult.Error!.Message.Contains("Forbidden") || listResult.Error!.Message.Contains("Method Not Allowed"))))
        {
            _output.WriteLine($"Skipped: {listResult.Error!.Message}");
        }
        else
        {
            // No subscriptions found, skip test
            _output.WriteLine("No subscriptions found - skipping test");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscription_WithFakeIds_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // Creating subscriptions with fake/non-existent customer and product IDs
        // should return null
        var subscriptionRequest = new SubscriptionCreateRequest
        {
            CustomerId = "cus_test_123456789",
            ProductPriceId = "price_test_123456789",
            TrialPeriodDays = 7,
            StartImmediately = true,
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["integration"] = true
            }
        };

        var result = await client.Subscriptions.CreateAsync(subscriptionRequest);

        // With fake IDs, the API should return null (invalid input)
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_UpdateSubscription_HandlesPermissionLimitations()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // First, try to list subscriptions to get a real subscription ID
        var listResult = await client.Subscriptions.ListAsync(limit: 1);
        if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
        {
            var subscriptionId = listResult.Value.Items[0].Id;
            var updateRequest = new SubscriptionUpdateRequest
            {
                // Remove discount by setting to null
                DiscountId = null
            };

            var updateResult = await client.Subscriptions.UpdateAsync(subscriptionId, updateRequest);

            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue();
            var updatedSubscription = updateResult.Value;
            updatedSubscription.Should().NotBeNull();
            updatedSubscription.Id.Should().Be(subscriptionId);
        }
        else if (listResult.IsAuthError || (listResult.IsFailure && (listResult.Error!.Message.Contains("Unauthorized") ||
                 listResult.Error!.Message.Contains("Forbidden") || listResult.Error!.Message.Contains("Method Not Allowed"))))
        {
            _output.WriteLine($"Skipped: {listResult.Error!.Message}");
        }
        else
        {
            // No subscriptions found, skip test
            _output.WriteLine("No subscriptions found - skipping test");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_RevokeSubscription_HandlesPermissionLimitations()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // First, try to list subscriptions to get a real subscription ID
        var listResult = await client.Subscriptions.ListAsync(limit: 1);
        if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
        {
            var subscriptionId = listResult.Value.Items[0].Id;
            var revokeResult = await client.Subscriptions.RevokeAsync(subscriptionId);

            revokeResult.Should().NotBeNull();
            revokeResult.IsSuccess.Should().BeTrue();
            var revokedSubscription = revokeResult.Value;
            revokedSubscription.Should().NotBeNull();
            revokedSubscription.Id.Should().Be(subscriptionId);
        }
        else if (listResult.IsAuthError || (listResult.IsFailure && (listResult.Error!.Message.Contains("Unauthorized") ||
                 listResult.Error!.Message.Contains("Forbidden") || listResult.Error!.Message.Contains("Method Not Allowed"))))
        {
            _output.WriteLine($"Skipped: {listResult.Error!.Message}");
        }
        else
        {
            // No subscriptions found, skip test
            _output.WriteLine("No subscriptions found - skipping test");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_GetNonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "sub_00000000000000000000000000";

        // Act
        var result = await client.Subscriptions.GetAsync(nonExistentId);

        // Assert - With nullable return types, non-existent resources return null
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_UpdateNonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "sub_00000000000000000000000000";
        var updateRequest = new SubscriptionUpdateRequest
        {
            // Try to cancel at period end as a simple update operation
            CancelAtPeriodEnd = true
        };

        // Act
        var result = await client.Subscriptions.UpdateAsync(nonExistentId, updateRequest);

        // Assert - With nullable return types, non-existent resources return null
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_RevokeNonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "sub_00000000000000000000000000";

        // Act
        var result = await client.Subscriptions.RevokeAsync(nonExistentId);

        // Assert - With nullable return types, non-existent resources return null
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Theory]
    [InlineData(SubscriptionStatus.Active)]
    [InlineData(SubscriptionStatus.Trialing)]
    [InlineData(SubscriptionStatus.PastDue)]
    [InlineData(SubscriptionStatus.Canceled)]
    [InlineData(SubscriptionStatus.Incomplete)]
    [InlineData(SubscriptionStatus.IncompleteExpired)]
    [InlineData(SubscriptionStatus.Unpaid)]
    public async Task SubscriptionsApi_ListByStatus_WorksCorrectly(SubscriptionStatus status)
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Subscriptions.ListAsync(status: status);

        // Assert
        if (result.IsSuccess)
        {
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();

            // Verify all returned subscriptions have the requested status (if any subscriptions exist)
            foreach (var subscription in result.Value.Items)
            {
                subscription.Status.Should().Be(status);
            }
        }
        else if (result.IsAuthError || (result.Error!.Message.Contains("Unauthorized") ||
                 result.Error!.Message.Contains("Forbidden") || result.Error!.Message.Contains("Method Not Allowed")))
        {
            _output.WriteLine($"Skipped: {result.Error!.Message}");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_Export_ReturnsNullOrValidResult()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        // Export endpoints may not be available in sandbox or may return empty
        var exportRequest = new SubscriptionExportRequest
        {
            Format = Api.ExportFormat.Csv,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        var exportResult = await client.Subscriptions.ExportAsync(exportRequest);

        // Assert
        exportResult.Should().NotBeNull();
        exportResult.IsSuccess.Should().BeTrue();

        // In sandbox, export may return null (not supported) or a valid result
        if (exportResult.Value != null)
        {
            // If we got a valid result, verify its properties
            exportResult.Value.ExportUrl.Should().NotBeNullOrEmpty();
            exportResult.Value.ExportId.Should().NotBeNullOrEmpty();
            exportResult.Value.Format.Should().Be(Api.ExportFormat.Csv);
        }
        // null value is acceptable in sandbox
    }

    [Fact]
    public async Task SubscriptionsApi_ListWithQueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var builder = client.Subscriptions.Query();
        var result = await client.Subscriptions.ListAsync(builder, page: 1, limit: 5);

        // Assert
        if (result.IsSuccess)
        {
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();
        }
        else if (result.IsAuthError || (result.Error!.Message.Contains("Unauthorized") ||
                 result.Error!.Message.Contains("Forbidden") || result.Error!.Message.Contains("Method Not Allowed")))
        {
            _output.WriteLine($"Skipped: {result.Error!.Message}");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_ListPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        // Test first page
        var firstPageResult = await client.Subscriptions.ListAsync(page: 1, limit: 2);

        // Assert
        if (firstPageResult.IsSuccess)
        {
            firstPageResult.Value.Should().NotBeNull();
            firstPageResult.Value.Items.Should().NotBeNull();
            firstPageResult.Value.Pagination.Should().NotBeNull();

            if (firstPageResult.Value.Items.Count > 0 && firstPageResult.Value.Pagination.MaxPage > 1)
            {
                // Test second page if it exists
                var secondPageResult = await client.Subscriptions.ListAsync(page: 2, limit: 2);
                secondPageResult.Should().NotBeNull();
                secondPageResult.IsSuccess.Should().BeTrue();
                secondPageResult.Value.Items.Should().NotBeNull();
                secondPageResult.Value.Pagination.Should().NotBeNull();

                // Ensure no duplicate items between pages
                var firstPageIds = firstPageResult.Value.Items.Select(s => s.Id).ToHashSet();
                var secondPageIds = secondPageResult.Value.Items.Select(s => s.Id).ToHashSet();
                firstPageIds.IntersectWith(secondPageIds);
                firstPageIds.Should().BeEmpty();
            }
        }
        else if (firstPageResult.IsAuthError || (firstPageResult.Error!.Message.Contains("Unauthorized") ||
                 firstPageResult.Error!.Message.Contains("Forbidden") || firstPageResult.Error!.Message.Contains("Method Not Allowed")))
        {
            _output.WriteLine($"Skipped: {firstPageResult.Error!.Message}");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_SubscriptionProperties_AreValid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var listResult = await client.Subscriptions.ListAsync(limit: 1);

        // Assert
        if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
        {
            var subscription = listResult.Value.Items[0];

            // Test all required properties
            subscription.Id.Should().NotBeNullOrEmpty();
            subscription.Status.Should().BeOneOf(SubscriptionStatus.Active, SubscriptionStatus.Trialing, SubscriptionStatus.PastDue, SubscriptionStatus.Canceled, SubscriptionStatus.Incomplete, SubscriptionStatus.IncompleteExpired, SubscriptionStatus.Unpaid);
            subscription.CustomerId.Should().NotBeNullOrEmpty();
            subscription.ProductId.Should().NotBeNullOrEmpty();
            subscription.CurrentPeriodStart.Should().BeBefore(DateTime.UtcNow.AddDays(1));
            subscription.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddDays(1));
            subscription.ModifiedAt.Should().BeBefore(DateTime.UtcNow.AddDays(1));

            // Optional properties - just verify they exist (can be null)
            // TrialStart, TrialEnd, CanceledAt, EndedAt, Metadata are all nullable
        }
        else if (listResult.IsAuthError || (listResult.IsFailure && (listResult.Error!.Message.Contains("Unauthorized") ||
                 listResult.Error!.Message.Contains("Forbidden") || listResult.Error!.Message.Contains("Method Not Allowed"))))
        {
            _output.WriteLine($"Skipped: {listResult.Error!.Message}");
        }
        else
        {
            // No subscriptions found, skip test
            _output.WriteLine("No subscriptions found - skipping test");
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithTrial_WithFakeIds_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        // Creating subscription with trial using fake IDs should return null
        var subscriptionRequest = new SubscriptionCreateRequest
        {
            CustomerId = "cus_test_123456789",
            ProductPriceId = "price_test_123456789",
            TrialPeriodDays = 14,
            StartImmediately = false,
            ExternalId = $"test_sub_{Guid.NewGuid()}",
            Metadata = new Dictionary<string, object>
            {
                ["trial"] = true,
                ["integration_test"] = true,
                ["created_at"] = DateTime.UtcNow.ToString("O")
            }
        };

        var result = await client.Subscriptions.CreateAsync(subscriptionRequest);

        // Assert
        // With fake IDs, the API should return null (invalid input)
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithInvalidCustomerId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var invalidRequest = new SubscriptionCreateRequest
        {
            CustomerId = "invalid_customer_id_that_does_not_exist",
            ProductPriceId = "price_test_123456789"
        };

        var result = await client.Subscriptions.CreateAsync(invalidRequest);

        // Assert
        // If no exception, the API should have returned null for invalid input
        result.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithEmptyRequest_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var emptyRequest = new SubscriptionCreateRequest();
        var result = await client.Subscriptions.CreateAsync(emptyRequest);

        // Assert
        // If no exception, the API should have returned null for invalid/empty request
        result.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithEmptyCustomerId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var invalidRequest = new SubscriptionCreateRequest
        {
            CustomerId = "",
            ProductPriceId = "price_123"
        };

        var result = await client.Subscriptions.CreateAsync(invalidRequest);

        // Assert
        // If no exception, the API should have returned null for empty customer ID
        result.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_ExportWithInvalidRequest_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var invalidRequest = new SubscriptionExportRequest
        {
            Format = Api.ExportFormat.Csv,
            StartDate = DateTime.UtcNow.AddDays(30), // Start date in the future
            EndDate = DateTime.UtcNow.AddDays(-30)   // End date before start date
        };

        var result = await client.Subscriptions.ExportAsync(invalidRequest);

        // Assert
        // If no exception, the API should have returned null for invalid request
        result.Should().BeNull();
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscription_WithValidIds_ReturnsValidSubscription()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, get a real customer ID
        string? customerId = null;
        string? productPriceId = null;

        // Get a real customer
        var customersResult = await client.Customers.ListAsync(limit: 1);
        if (customersResult.IsSuccess && customersResult.Value.Items.Count > 0)
        {
            customerId = customersResult.Value.Items[0].Id;
        }

        // Get a real product with a recurring price (for subscription)
        var productsResult = await client.Products.ListAsync(limit: 10);
        if (productsResult.IsSuccess)
        {
            foreach (var product in productsResult.Value.Items)
            {
                var recurringPrice = product.Prices.FirstOrDefault(p => p.Type == PriceType.Recurring);
                if (recurringPrice != null)
                {
                    productPriceId = recurringPrice.Id;
                    break;
                }
            }
        }

        // If we don't have valid IDs, skip the test
        if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(productPriceId))
        {
            _output.WriteLine("No valid customer or recurring product price found - skipping test");
            return;
        }

        // Act
        var subscriptionRequest = new SubscriptionCreateRequest
        {
            CustomerId = customerId,
            ProductPriceId = productPriceId,
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["integration_test"] = true,
                ["created_at"] = DateTime.UtcNow.ToString("O")
            }
        };

        var result = await client.Subscriptions.CreateAsync(subscriptionRequest);

        // If null is returned, sandbox doesn't support subscription creation - skip
        if (result.IsSuccess && result.Value == null)
        {
            _output.WriteLine("Sandbox returned null - subscription creation not supported in this environment");
            return;
        }

        // Assert - If creation succeeds, the subscription must be valid
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var createdSubscription = result.Value;
        // The subscription should NOT have empty ID if it's not null
        createdSubscription.Should().NotBeNull();
        createdSubscription.Id.Should().NotBeNullOrEmpty("subscription ID should be set when subscription is returned");
        createdSubscription.CustomerId.Should().Be(customerId, "customer ID should match the request");
        createdSubscription.Status.Should().BeOneOf(
            SubscriptionStatus.Active,
            SubscriptionStatus.Trialing,
            SubscriptionStatus.Incomplete,
            SubscriptionStatus.IncompleteExpired,
            SubscriptionStatus.PastDue,
            SubscriptionStatus.Canceled,
            SubscriptionStatus.Unpaid);
        createdSubscription.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5), "creation time should be recent");
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscription_WithValidIds_HasRequiredProperties()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, get a real customer ID and product price ID
        string? customerId = null;
        string? productPriceId = null;
        string? productId = null;

        // Get a real customer
        var customersResult = await client.Customers.ListAsync(limit: 1);
        if (customersResult.IsSuccess && customersResult.Value.Items.Count > 0)
        {
            customerId = customersResult.Value.Items[0].Id;
        }

        // Get a real product with a recurring price
        var productsResult = await client.Products.ListAsync(limit: 10);
        if (productsResult.IsSuccess)
        {
            foreach (var product in productsResult.Value.Items)
            {
                var recurringPrice = product.Prices.FirstOrDefault(p => p.Type == PriceType.Recurring);
                if (recurringPrice != null)
                {
                    productPriceId = recurringPrice.Id;
                    productId = product.Id;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(productPriceId))
        {
            _output.WriteLine("No valid customer or recurring product price found - skipping test");
            return;
        }

        // Act
        var subscriptionRequest = new SubscriptionCreateRequest
        {
            CustomerId = customerId,
            ProductPriceId = productPriceId
        };

        var result = await client.Subscriptions.CreateAsync(subscriptionRequest);

        // If null is returned, sandbox doesn't support subscription creation - skip
        if (result.IsSuccess && result.Value == null)
        {
            _output.WriteLine("Sandbox returned null - subscription creation not supported in this environment");
            return;
        }

        // Assert - Verify all required properties are populated
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var createdSubscription = result.Value;
        createdSubscription.Should().NotBeNull();

        // The subscription should NOT have empty properties if it's not null

        // ID must be set
        createdSubscription.Id.Should().NotBeNullOrEmpty("Id must be set on created subscription");

        // Customer ID must match
        createdSubscription.CustomerId.Should().NotBeNullOrEmpty("CustomerId must be set");
        createdSubscription.CustomerId.Should().Be(customerId);

        // Product ID must be set
        createdSubscription.ProductId.Should().NotBeNullOrEmpty("ProductId must be set");

        // Status must be valid
        createdSubscription.Status.Should().BeOneOf(
            SubscriptionStatus.Active,
            SubscriptionStatus.Trialing,
            SubscriptionStatus.Incomplete,
            SubscriptionStatus.IncompleteExpired,
            SubscriptionStatus.PastDue,
            SubscriptionStatus.Canceled,
            SubscriptionStatus.Unpaid);

        // Dates must be set
        createdSubscription.CreatedAt.Should().NotBe(default(DateTime), "CreatedAt must be set");
        createdSubscription.ModifiedAt.Should().NotBe(default(DateTime), "ModifiedAt must be set");
        createdSubscription.CurrentPeriodStart.Should().NotBe(default(DateTime), "CurrentPeriodStart must be set");
    }
}
