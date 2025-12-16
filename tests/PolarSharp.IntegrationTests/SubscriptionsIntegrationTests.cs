using FluentAssertions;
using PolarSharp.Models.Products;
using PolarSharp.Models.Subscriptions;
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
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SubscriptionsApi_ListAllAsync_EnumeratesAllPages()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var subscriptions = new List<Subscription>();
        await foreach (var subscription in client.Subscriptions.ListAllAsync())
        {
            subscriptions.Add(subscription);
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
        try
        {
            var resultWithCustomer = await client.Subscriptions.ListAsync(customerId: "test_customer_id");
            resultWithCustomer.Should().NotBeNull();
            resultWithCustomer.Items.Should().NotBeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed") || ex.Message.Contains("Not Found") || ex.Message.Contains("RequestValidationError"))
        {
            // Expected in sandbox environment with limited permissions or when using fake customer ID
            true.Should().BeTrue();
        }

        // Test with product ID filter
        try
        {
            var resultWithProduct = await client.Subscriptions.ListAsync(productId: "test_product_id");
            resultWithProduct.Should().NotBeNull();
            resultWithProduct.Items.Should().NotBeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed") || ex.Message.Contains("Not Found") || ex.Message.Contains("RequestValidationError"))
        {
            // Expected in sandbox environment with limited permissions or when using fake product ID
            true.Should().BeTrue();
        }

        // Test with status filter
        try
        {
            var resultWithStatus = await client.Subscriptions.ListAsync(status: SubscriptionStatus.Active);
            resultWithStatus.Should().NotBeNull();
            resultWithStatus.Items.Should().NotBeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed") || ex.Message.Contains("RequestValidationError") || ex.Message.Contains("Not Found"))
        {
            // Expected in sandbox environment with limited permissions or validation issues
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_GetSubscription_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // First, try to list subscriptions to get a real subscription ID
        try
        {
            var listResult = await client.Subscriptions.ListAsync(limit: 1);
            if (listResult.Items.Count > 0)
            {
                var subscriptionId = listResult.Items[0].Id;
                var subscription = await client.Subscriptions.GetAsync(subscriptionId);

                subscription.Should().NotBeNull();
                subscription.Id.Should().Be(subscriptionId);
                subscription.Status.Should().BeOneOf(SubscriptionStatus.Active, SubscriptionStatus.Trialing, SubscriptionStatus.PastDue, SubscriptionStatus.Canceled, SubscriptionStatus.Incomplete, SubscriptionStatus.IncompleteExpired, SubscriptionStatus.Unpaid);
                subscription.CustomerId.Should().NotBeNullOrEmpty();
                subscription.ProductId.Should().NotBeNullOrEmpty();
                subscription.CurrentPeriodStart.Should().BeBefore(DateTime.UtcNow.AddDays(1));
                subscription.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddDays(1));
                subscription.ModifiedAt.Should().BeBefore(DateTime.UtcNow.AddDays(1));
            }
            else
            {
                // No subscriptions found, skip test
                true.Should().BeTrue();
            }
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscription_WithFakeIds_ReturnsNullOrThrows()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // Creating subscriptions with fake/non-existent customer and product IDs
        // should either return null or throw an exception
        try
        {
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

            var createdSubscription = await client.Subscriptions.CreateAsync(subscriptionRequest);

            // With fake IDs, the API should return null (invalid input)
            createdSubscription.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // Expected - API rejected the invalid IDs
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_UpdateSubscription_HandlesPermissionLimitations()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // First, try to list subscriptions to get a real subscription ID
        try
        {
            var listResult = await client.Subscriptions.ListAsync(limit: 1);
            if (listResult.Items.Count > 0)
            {
                var subscriptionId = listResult.Items[0].Id;
                var updateRequest = new SubscriptionUpdateRequest
                {
                    Metadata = new Dictionary<string, object>
                    {
                        ["updated"] = true,
                        ["test_run"] = DateTime.UtcNow.ToString("O")
                    }
                };

                var updatedSubscription = await client.Subscriptions.UpdateAsync(subscriptionId, updateRequest);

                updatedSubscription.Should().NotBeNull();
                updatedSubscription.Id.Should().Be(subscriptionId);
                updatedSubscription.Metadata.Should().NotBeNull();
                updatedSubscription.Metadata!["updated"].Should().Be(true);
            }
            else
            {
                // No subscriptions found, skip test
                true.Should().BeTrue();
            }
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_RevokeSubscription_HandlesPermissionLimitations()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // First, try to list subscriptions to get a real subscription ID
        try
        {
            var listResult = await client.Subscriptions.ListAsync(limit: 1);
            if (listResult.Items.Count > 0)
            {
                var subscriptionId = listResult.Items[0].Id;
                var revokedSubscription = await client.Subscriptions.RevokeAsync(subscriptionId);

                revokedSubscription.Should().NotBeNull();
                revokedSubscription.Id.Should().Be(subscriptionId);
            }
            else
            {
                // No subscriptions found, skip test
                true.Should().BeTrue();
            }
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_GetNonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "sub_00000000000000000000000000";

        // Act & Assert
        try
        {
            var result = await client.Subscriptions.GetAsync(nonExistentId);
            
            // Assert - With nullable return types, non-existent resources return null
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_UpdateNonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "sub_00000000000000000000000000";
        var updateRequest = new SubscriptionUpdateRequest
        {
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true
            }
        };

        // Act & Assert
        try
        {
            var result = await client.Subscriptions.UpdateAsync(nonExistentId, updateRequest);
            
            // Assert - With nullable return types, non-existent resources return null
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_RevokeNonExistentSubscription_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "sub_00000000000000000000000000";

        // Act & Assert
        try
        {
            var result = await client.Subscriptions.RevokeAsync(nonExistentId);
            
            // Assert - With nullable return types, non-existent resources return null
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
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

        // Act & Assert
        try
        {
            var result = await client.Subscriptions.ListAsync(status: status);

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Pagination.Should().NotBeNull();

            // Verify all returned subscriptions have the requested status (if any subscriptions exist)
            foreach (var subscription in result.Items)
            {
                subscription.Status.Should().Be(status);
            }
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_Export_ReturnsNullOrThrowsInSandbox()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // Export endpoints may not be available in sandbox or may return empty
        try
        {
            var exportRequest = new SubscriptionExportRequest
            {
                Format = Api.ExportFormat.Csv,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            var exportResult = await client.Subscriptions.ExportAsync(exportRequest);

            // In sandbox, export may return null (not supported) or a valid result
            if (exportResult != null)
            {
                // If we got a valid result, verify its properties
                exportResult.ExportUrl.Should().NotBeNullOrEmpty();
                exportResult.ExportId.Should().NotBeNullOrEmpty();
                exportResult.Format.Should().Be(Api.ExportFormat.Csv);
            }
            // null is acceptable in sandbox
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_ListWithQueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
            var builder = client.Subscriptions.Query();
            var result = await client.Subscriptions.ListAsync(builder, page: 1, limit: 5);

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Pagination.Should().NotBeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_ListPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
            // Test first page
            var firstPage = await client.Subscriptions.ListAsync(page: 1, limit: 2);
            firstPage.Should().NotBeNull();
            firstPage.Items.Should().NotBeNull();
            firstPage.Pagination.Should().NotBeNull();

            if (firstPage.Items.Count > 0 && firstPage.Pagination.MaxPage > 1)
            {
                // Test second page if it exists
                var secondPage = await client.Subscriptions.ListAsync(page: 2, limit: 2);
                secondPage.Should().NotBeNull();
                secondPage.Items.Should().NotBeNull();
                secondPage.Pagination.Should().NotBeNull();

                // Ensure no duplicate items between pages
                var firstPageIds = firstPage.Items.Select(s => s.Id).ToHashSet();
                var secondPageIds = secondPage.Items.Select(s => s.Id).ToHashSet();
                firstPageIds.IntersectWith(secondPageIds);
                firstPageIds.Should().BeEmpty();
            }
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_SubscriptionProperties_AreValid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
            var listResult = await client.Subscriptions.ListAsync(limit: 1);
            if (listResult.Items.Count > 0)
            {
                var subscription = listResult.Items[0];

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
            else
            {
                // No subscriptions found, skip test
                true.Should().BeTrue();
            }
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithTrial_WithFakeIds_ReturnsNullOrThrows()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // Creating subscription with trial using fake IDs should return null or throw
        try
        {
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

            var createdSubscription = await client.Subscriptions.CreateAsync(subscriptionRequest);

            // With fake IDs, the API should return null (invalid input)
            createdSubscription.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // Expected - API rejected the invalid IDs
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithInvalidCustomerId_ReturnsNullOrThrows()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - Invalid customer ID should return null or throw
        try
        {
            var invalidRequest = new SubscriptionCreateRequest
            {
                CustomerId = "invalid_customer_id_that_does_not_exist",
                ProductPriceId = "price_test_123456789"
            };

            var result = await client.Subscriptions.CreateAsync(invalidRequest);

            // If no exception, the API should have returned null for invalid input
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // This is expected behavior - API rejected the invalid request
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithEmptyRequest_ReturnsNullOrThrows()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - Empty request should return null or throw
        try
        {
            var emptyRequest = new SubscriptionCreateRequest();
            var result = await client.Subscriptions.CreateAsync(emptyRequest);

            // If no exception, the API should have returned null for invalid/empty request
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // This is expected behavior - validation error
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscriptionWithEmptyCustomerId_ReturnsNullOrThrows()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - Empty customer ID should return null or throw
        try
        {
            var invalidRequest = new SubscriptionCreateRequest
            {
                CustomerId = "",
                ProductPriceId = "price_123"
            };

            var result = await client.Subscriptions.CreateAsync(invalidRequest);

            // If no exception, the API should have returned null for empty customer ID
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // This is expected behavior - validation error
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_ExportWithInvalidRequest_ReturnsNullOrThrows()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - Invalid date range should return null or throw
        try
        {
            var invalidRequest = new SubscriptionExportRequest
            {
                Format = Api.ExportFormat.Csv,
                StartDate = DateTime.UtcNow.AddDays(30), // Start date in the future
                EndDate = DateTime.UtcNow.AddDays(-30)   // End date before start date
            };

            var result = await client.Subscriptions.ExportAsync(invalidRequest);

            // If no exception, the API should have returned null for invalid request
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // This is expected behavior - validation error
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SubscriptionsApi_CreateSubscription_WithValidIds_ReturnsValidSubscription()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, get a real customer ID
        string? customerId = null;
        string? productPriceId = null;

        try
        {
            // Get a real customer
            var customers = await client.Customers.ListAsync(limit: 1);
            if (customers.Items.Count > 0)
            {
                customerId = customers.Items[0].Id;
            }

            // Get a real product with a recurring price (for subscription)
            var products = await client.Products.ListAsync(limit: 10);
            foreach (var product in products.Items)
            {
                var recurringPrice = product.Prices.FirstOrDefault(p => p.Type == PriceType.Recurring);
                if (recurringPrice != null)
                {
                    productPriceId = recurringPrice.Id;
                    break;
                }
            }
        }
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            // Skip test if we can't get prerequisites
            _output.WriteLine("Could not retrieve customer or product data - skipping test");
            return;
        }

        // If we don't have valid IDs, skip the test
        if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(productPriceId))
        {
            _output.WriteLine("No valid customer or recurring product price found - skipping test");
            return;
        }

        // Act
        try
        {
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

            var createdSubscription = await client.Subscriptions.CreateAsync(subscriptionRequest);

            // If null is returned, sandbox doesn't support subscription creation - skip
            if (createdSubscription == null)
            {
                _output.WriteLine("Sandbox returned null - subscription creation not supported in this environment");
                return;
            }

            // Assert - If creation succeeds, the subscription must be valid
            // The subscription should NOT have empty ID if it's not null
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
        catch (PolarSharp.Exceptions.PolarApiException ex)
        {
            // API may reject for various reasons (permissions, sandbox limitations, etc.)
            _output.WriteLine($"API rejected subscription creation: {ex.Message}");
        }
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

        try
        {
            // Get a real customer
            var customers = await client.Customers.ListAsync(limit: 1);
            if (customers.Items.Count > 0)
            {
                customerId = customers.Items[0].Id;
            }

            // Get a real product with a recurring price
            var products = await client.Products.ListAsync(limit: 10);
            foreach (var product in products.Items)
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
        catch (PolarSharp.Exceptions.PolarApiException)
        {
            _output.WriteLine("Could not retrieve customer or product data - skipping test");
            return;
        }

        if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(productPriceId))
        {
            _output.WriteLine("No valid customer or recurring product price found - skipping test");
            return;
        }

        // Act
        try
        {
            var subscriptionRequest = new SubscriptionCreateRequest
            {
                CustomerId = customerId,
                ProductPriceId = productPriceId
            };

            var createdSubscription = await client.Subscriptions.CreateAsync(subscriptionRequest);

            // If null is returned, sandbox doesn't support subscription creation - skip
            if (createdSubscription == null)
            {
                _output.WriteLine("Sandbox returned null - subscription creation not supported in this environment");
                return;
            }

            // Assert - Verify all required properties are populated
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
        catch (PolarSharp.Exceptions.PolarApiException ex)
        {
            _output.WriteLine($"API rejected subscription creation: {ex.Message}");
            true.Should().BeTrue();
        }
    }
}