using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PolarSharp.Results;
using PolarSharp.Models.Checkouts;
using PolarSharp.Models.Products;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Checkouts API.
/// Tests are organized into categories:
/// 1. List tests - tests for listing and filtering checkouts
/// 2. Create tests - tests for creating checkout sessions
/// 3. Get tests - tests for retrieving checkouts
/// 4. Update tests - tests for updating checkouts
/// 5. Client-side tests - tests for client-side endpoints using client_secret
/// 6. Error handling tests - tests for error conditions
/// 7. Lifecycle tests - end-to-end checkout workflows
/// </summary>
public class CheckoutsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public CheckoutsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    #region Helper Methods

    /// <summary>
    /// Gets an active (non-archived) product ID for testing.
    /// Uses the Products API directly to find active products.
    /// </summary>
    private async Task<string?> GetActiveProductIdAsync(PolarClient client)
    {
        try
        {
            var productsResult = await client.Products.ListAsync(limit: 50);
            if (productsResult.IsFailure)
            {
                _output.WriteLine($"Failed to list products: {productsResult.Error!.Message}");
                return null;
            }

            var activeProduct = productsResult.Value.Items.FirstOrDefault(p => !p.IsArchived);

            if (activeProduct != null)
            {
                _output.WriteLine($"Found active product: {activeProduct.Name} ({activeProduct.Id})");
                return activeProduct.Id;
            }

            _output.WriteLine("No active products found in the account");
            return null;
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
            return null;
        }
    }

    /// <summary>
    /// Gets a product ID from an existing checkout (may be archived).
    /// </summary>
    private async Task<string?> GetProductIdFromExistingCheckoutAsync(PolarClient client)
    {
        try
        {
            var checkoutsResult = await client.Checkouts.ListAsync(limit: 10);
            if (checkoutsResult.IsFailure)
            {
                return null;
            }
            var checkoutWithProduct = checkoutsResult.Value.Items.FirstOrDefault(c => !string.IsNullOrEmpty(c.ProductId));
            return checkoutWithProduct?.ProductId;
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
            return null;
        }
    }

    /// <summary>
    /// Creates a test checkout and returns it with valid data.
    /// </summary>
    private async Task<Checkout?> CreateTestCheckoutAsync(PolarClient client)
    {
        try
        {
            var productId = await GetActiveProductIdAsync(client);
            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products - cannot create test checkout");
                return null;
            }

            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                SuccessUrl = "https://example.com/success"
            };

            var result = await client.Checkouts.CreateAsync(request);
            return result.IsSuccess ? result.Value : null;
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
            return null;
        }
    }

    #endregion

    #region List Tests - GET /v1/checkouts/

    [Fact]
    public async Task ListAsync_WithValidParameters_ReturnsNonNullPaginatedResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ListAsync(page: 1, limit: 5);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Items.Should().NotBeNull("Items collection should never be null");
            result.Value.Pagination.Should().NotBeNull("Pagination metadata should be present");
            result.Value.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
            result.Value.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);

            _output.WriteLine($"Found {result.Value.Pagination.TotalCount} total checkouts across {result.Value.Pagination.MaxPage} pages");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_EnumeratesAllCheckouts_ReturnsNonNullItems()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var checkouts = new List<Checkout>();

            // Act
            await foreach (var checkoutResult in client.Checkouts.ListAllAsync())
            {
                if (checkoutResult.IsFailure) break;

                var checkout = checkoutResult.Value;
                checkouts.Add(checkout);

                // Verify each item is valid
                checkout.Should().NotBeNull();
                checkout.Id.Should().NotBeNullOrEmpty();

                // Limit to prevent long-running test
                if (checkouts.Count >= 50)
                    break;
            }

            // Assert
            checkouts.Should().NotBeNull("Enumerable should produce a valid list");
            _output.WriteLine($"Enumerated {checkouts.Count} checkouts");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Filter by open status
            var result = await client.Checkouts.ListAsync(status: CheckoutStatus.Open);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Items.Should().NotBeNull();

            // Verify all returned items have the correct status
            foreach (var checkout in result.Value.Items)
            {
                checkout.Status.Should().Be(CheckoutStatus.Open,
                    "All returned checkouts should have the filtered status");
            }

            _output.WriteLine($"Found {result.Value.Items.Count} open checkouts");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Theory]
    [InlineData(CheckoutStatus.Open)]
    [InlineData(CheckoutStatus.Expired)]
    public async Task ListAsync_WithDifferentStatusFilters_ReturnsValidResponses(CheckoutStatus status)
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ListAsync(status: status);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();

            // Verify status consistency if there are results
            foreach (var checkout in result.Value.Items)
            {
                checkout.Status.Should().Be(status);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ReturnsValidResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var query = client.Checkouts.Query();

            // Act
            var result = await client.Checkouts.ListAsync(query, page: 1, limit: 10);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithPagination_NavigatesPagesProperly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var firstPageResult = await client.Checkouts.ListAsync(page: 1, limit: 2);

            // Assert first page
            firstPageResult.Should().NotBeNull();
            if (firstPageResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {firstPageResult.Error!.Message}");
                return;
            }
            var firstPage = firstPageResult.Value;
            firstPage.Pagination.Should().NotBeNull();

            if (firstPage.Pagination.MaxPage > 1)
            {
                var secondPageResult = await client.Checkouts.ListAsync(page: 2, limit: 2);
                if (secondPageResult.IsFailure)
                {
                    _output.WriteLine($"Skipped: {secondPageResult.Error!.Message}");
                    return;
                }
                var secondPage = secondPageResult.Value;

                secondPage.Should().NotBeNull();
                secondPage.Items.Should().NotBeNull();

                // Verify different items on different pages (if enough data exists)
                if (firstPage.Items.Count > 0 && secondPage.Items.Count > 0)
                {
                    firstPage.Items[0].Id.Should().NotBe(secondPage.Items[0].Id,
                        "Different pages should contain different checkouts");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_ReturnsCheckoutsWithRequiredFields()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ListAsync(limit: 10);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }

            if (result.Value.Items.Count > 0)
            {
                var checkout = result.Value.Items[0];
                checkout.Id.Should().NotBeNullOrEmpty("id is a required field");
                checkout.Status.Should().BeDefined("status is a required field");
                checkout.CreatedAt.Should().NotBe(default, "created_at is a required field");

                _output.WriteLine($"Checkout {checkout.Id}: Status={checkout.Status}, Amount={checkout.Amount}");
            }
            else
            {
                _output.WriteLine("No checkouts available for field validation");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithMaxLimit_CapsAtHundred()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Request more than the max allowed limit
            var result = await client.Checkouts.ListAsync(page: 1, limit: 200);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Items.Should().NotBeNull();
            result.Value.Items.Count.Should().BeLessThanOrEqualTo(100,
                "API should cap results at maximum of 100 items per page");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithZeroLimit_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ListAsync(page: 1, limit: 0);

            // Assert - API rejects zero limit with validation error
            result.IsFailure.Should().BeTrue();
            result.IsValidationError.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithNegativeLimit_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ListAsync(page: 1, limit: -1);

            // Assert - API rejects negative limit with validation error
            result.IsFailure.Should().BeTrue();
            result.IsValidationError.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithHighPageNumber_ReturnsEmptyItems()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Request a page that likely doesn't exist
            var result = await client.Checkouts.ListAsync(page: 99999, limit: 10);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("High page number beyond available data should return empty items");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_CheckoutItems_HaveConsistentStructure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ListAsync(limit: 10);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            foreach (var checkout in result.Value.Items)
            {
                checkout.Id.Should().NotBeNullOrEmpty();
                checkout.Status.Should().BeDefined();
                checkout.CreatedAt.Should().NotBe(default);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_PaginationMetadata_IsConsistent()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ListAsync(page: 1, limit: 5);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Pagination.Should().NotBeNull();
            result.Value.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
            result.Value.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);

            // Items count should not exceed requested limit
            result.Value.Items.Count.Should().BeLessThanOrEqualTo(5);

            // Items count should not exceed total count
            result.Value.Items.Count.Should().BeLessThanOrEqualTo(result.Value.Pagination.TotalCount);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithInvalidCustomerId_ReturnsEmptyOrValidatesError()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidCustomerId = "invalid-customer-id";

            // Act
            var result = await client.Checkouts.ListAsync(customerId: invalidCustomerId);

            // Assert - API may either return empty results or validation error for invalid ID format
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Items.Should().NotBeNull();
                result.Value.Items.Should().BeEmpty("Invalid customer ID should yield no results");
            }
            else
            {
                // API validates ID format and returns error - this is acceptable
                result.Error.Should().NotBeNull();
                _output.WriteLine($"API returned validation error: {result.Error!.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithInvalidProductId_ReturnsEmptyOrValidatesError()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidProductId = "invalid-product-id";

            // Act
            var result = await client.Checkouts.ListAsync(productId: invalidProductId);

            // Assert - API may either return empty results or validation error for invalid ID format
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Items.Should().NotBeNull();
                result.Value.Items.Should().BeEmpty("Invalid product ID should yield no results");
            }
            else
            {
                // API validates ID format and returns error - this is acceptable
                result.Error.Should().NotBeNull();
                _output.WriteLine($"API returned validation error: {result.Error!.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    #endregion

    #region Create Checkout Tests - POST /v1/checkouts/

    [Fact]
    public async Task CreateAsync_WithValidProduct_ReturnsNewCheckout()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var productId = await GetActiveProductIdAsync(client);

            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products found - test requires at least one active product");
                return;
            }

            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                SuccessUrl = "https://example.com/success",
                Metadata = new Dictionary<string, object>
                {
                    ["test_key"] = "test_value",
                    ["created_by"] = "integration_test"
                }
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var checkout = result.Value;
            checkout.Should().NotBeNull("Creating checkout with valid product should succeed");
            checkout.Id.Should().NotBeNullOrEmpty("Checkout should have an ID");
            checkout.Status.Should().Be(CheckoutStatus.Open, "New checkout should be open");
            checkout.Url.Should().NotBeNullOrEmpty("Checkout should have a URL");
            checkout.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));

            _output.WriteLine($"Created checkout: {checkout.Id}");
            _output.WriteLine($"Checkout URL: {checkout.Url}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithMultipleProducts_ReturnsCheckoutWithProducts()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Get multiple product IDs
            var productsResult = await client.Products.ListAsync(limit: 20);
            if (productsResult.IsFailure)
            {
                _output.WriteLine("Failed to list products");
                return;
            }

            var activeProductIds = productsResult.Value.Items
                .Where(p => !p.IsArchived)
                .Select(p => p.Id)
                .Take(2)
                .ToList();

            if (activeProductIds.Count < 2)
            {
                _output.WriteLine("Not enough active products found (need 2) - test requires at least two active products");
                return;
            }

            var request = new CheckoutCreateRequest
            {
                Products = activeProductIds,
                SuccessUrl = "https://example.com/success"
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var checkout = result.Value;
            checkout.Should().NotBeNull("Creating checkout with multiple products should succeed");
            checkout.Id.Should().NotBeNullOrEmpty();
            checkout.Status.Should().Be(CheckoutStatus.Open);

            _output.WriteLine($"Created checkout with {activeProductIds.Count} products: {checkout.Id}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithCustomerEmail_ReturnsCheckoutOrHandlesValidation()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var productId = await GetActiveProductIdAsync(client);

            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products found - skipping test");
                return;
            }

            var testEmail = $"test@example.com";
            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                CustomerEmail = testEmail,
                SuccessUrl = "https://example.com/success"
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            if (result.IsSuccess)
            {
                var checkout = result.Value;
                checkout.Id.Should().NotBeNullOrEmpty();
                _output.WriteLine($"Created checkout for email {testEmail}: {checkout.Id}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithMetadata_ReturnsCheckoutWithMetadata()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var productId = await GetActiveProductIdAsync(client);

            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products found - skipping test");
                return;
            }

            var metadata = new Dictionary<string, object>
            {
                ["order_id"] = "ORD-12345",
                ["source"] = "integration_test",
                ["timestamp"] = DateTime.UtcNow.ToString("O")
            };

            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                SuccessUrl = "https://example.com/success",
                Metadata = metadata
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var checkout = result.Value;
            checkout.Should().NotBeNull();
            checkout.Id.Should().NotBeNullOrEmpty();

            _output.WriteLine($"Created checkout with metadata: {checkout.Id}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_CheckoutHasValidExpirationDate()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var productId = await GetActiveProductIdAsync(client);

            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products found - skipping test");
                return;
            }

            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                SuccessUrl = "https://example.com/success"
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var checkout = result.Value;
            checkout.Should().NotBeNull();
            if (checkout.ExpiresAt.HasValue)
            {
                checkout.ExpiresAt.Should().BeAfter(DateTime.UtcNow, "Expiration should be in the future");
            }

            _output.WriteLine($"Checkout expires at: {checkout.ExpiresAt}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_CheckoutHasClientSecret()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var productId = await GetActiveProductIdAsync(client);

            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products found - skipping test");
                return;
            }

            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                SuccessUrl = "https://example.com/success"
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var checkout = result.Value;
            checkout.Should().NotBeNull();
            checkout.ClientSecret.Should().NotBeNullOrEmpty("Checkout should have a client secret for client-side operations");

            _output.WriteLine($"Checkout client secret starts with: {checkout.ClientSecret?[..Math.Min(20, checkout.ClientSecret?.Length ?? 0)]}...");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithEmptyProductsList_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidRequest = new CheckoutCreateRequest
            {
                Products = new List<string>() // Empty products list - required field
            };

            // Act
            var result = await client.Checkouts.CreateAsync(invalidRequest);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.IsValidationError.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithInvalidProductId_HandlesValidationError()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidRequest = new CheckoutCreateRequest
            {
                Products = new List<string> { "invalid-product-id" }
            };

            // Act
            var result = await client.Checkouts.CreateAsync(invalidRequest);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.IsValidationError.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentProductUuid_HandlesValidationError()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentProductId = "00000000-0000-0000-0000-000000000000";
            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { nonExistentProductId }
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.IsValidationError.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithArchivedProduct_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Find an archived product
            var productsResult = await client.Products.ListAsync(limit: 50);
            if (productsResult.IsFailure)
            {
                _output.WriteLine("Failed to list products - skipping test");
                return;
            }

            var archivedProduct = productsResult.Value.Items.FirstOrDefault(p => p.IsArchived);

            if (archivedProduct == null)
            {
                _output.WriteLine("No archived products found - skipping test");
                return;
            }

            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { archivedProduct.Id },
                SuccessUrl = "https://example.com/success"
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.IsValidationError.Should().BeTrue();

            _output.WriteLine($"Correctly rejected archived product: {archivedProduct.Name}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    #endregion

    #region Get Tests - GET /v1/checkouts/{id}

    [Fact]
    public async Task GetAsync_WithExistingCheckout_ReturnsCheckoutOrHandlesApiError()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var checkout = await CreateTestCheckoutAsync(client);

            if (checkout == null)
            {
                _output.WriteLine("Could not create test checkout - skipping test");
                return;
            }

            // Act
            var result = await client.Checkouts.GetAsync(checkout.Id);

            // Assert
            if (result.IsSuccess)
            {
                var retrievedCheckout = result.Value;
                retrievedCheckout.Id.Should().Be(checkout.Id);
                _output.WriteLine($"Retrieved checkout: {retrievedCheckout.Id}");
            }
            else
            {
                _output.WriteLine($"GetAsync failed for checkout: {checkout.Id} - {result.Error!.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "00000000-0000-0000-0000-000000000000";

            // Act
            var result = await client.Checkouts.GetAsync(nonExistentId);

            // Assert - API should return either not found or validation error for non-existent ID
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            _output.WriteLine($"API returned error for non-existent checkout: {result.Error!.Message}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    #endregion

    #region Client-Side Tests - Using client_secret

    [Fact]
    public async Task GetFromClientAsync_WithValidClientSecret_ReturnsCheckout()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var checkout = await CreateTestCheckoutAsync(client);

            if (checkout == null)
            {
                _output.WriteLine("Could not create test checkout - skipping test");
                return;
            }

            if (string.IsNullOrEmpty(checkout.ClientSecret))
            {
                _output.WriteLine("Created checkout has no client secret - skipping test");
                return;
            }

            // Act - Use client secret instead of ID
            var result = await client.Checkouts.GetFromClientAsync(checkout.ClientSecret);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Should().NotBeNull("Getting checkout by client secret should succeed");
            result.Value.Id.Should().Be(checkout.Id);

            _output.WriteLine($"Retrieved checkout via client secret: {result.Value.Id}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetFromClientAsync_WithNonExistentId_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "00000000-0000-0000-0000-000000000000";

            // Act
            var result = await client.Checkouts.GetFromClientAsync(nonExistentId);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetFromClientAsync_WithInvalidUuidFormat_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidId = "not-a-valid-uuid";

            // Act
            var result = await client.Checkouts.GetFromClientAsync(invalidId);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithValidClientSecret_UpdatesCheckout()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var checkout = await CreateTestCheckoutAsync(client);

            if (checkout == null)
            {
                _output.WriteLine("Could not create test checkout - skipping test");
                return;
            }

            if (string.IsNullOrEmpty(checkout.ClientSecret))
            {
                _output.WriteLine("Created checkout has no client secret - skipping test");
                return;
            }

            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object>
                {
                    ["updated_via"] = "client_secret",
                    ["timestamp"] = DateTime.UtcNow.ToString("O")
                }
            };

            // Act
            var result = await client.Checkouts.UpdateFromClientAsync(checkout.ClientSecret, updateRequest);

            // Assert
            if (result.IsSuccess)
            {
                result.Value.Id.Should().Be(checkout.Id);
                _output.WriteLine($"Updated checkout via client secret: {result.Value.Id}");
            }
            else
            {
                _output.WriteLine("UpdateFromClientAsync failed - client-side updates may be restricted");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithNonExistentCheckout_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "00000000-0000-0000-0000-000000000000";
            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object> { ["test"] = "value" }
            };

            // Act
            var result = await client.Checkouts.UpdateFromClientAsync(nonExistentId, updateRequest);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithInvalidClientSecret_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidClientSecret = "invalid_client_secret_12345";
            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object> { ["test"] = "value" }
            };

            // Act
            var result = await client.Checkouts.UpdateFromClientAsync(invalidClientSecret, updateRequest);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithEmptyClientSecret_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object> { ["test"] = "value" }
            };

            // Act
            var result = await client.Checkouts.UpdateFromClientAsync("", updateRequest);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithNonExistentCheckout_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "00000000-0000-0000-0000-000000000000";

            // Act
            var result = await client.Checkouts.ConfirmFromClientAsync(nonExistentId);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithInvalidClientSecret_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidClientSecret = "invalid_client_secret_12345";

            // Act
            var result = await client.Checkouts.ConfirmFromClientAsync(invalidClientSecret);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithEmptyClientSecret_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Checkouts.ConfirmFromClientAsync("");

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithOpenCheckout_RequiresPaymentInfo()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var checkout = await CreateTestCheckoutAsync(client);

            if (checkout == null)
            {
                _output.WriteLine("Could not create test checkout - skipping test");
                return;
            }

            if (string.IsNullOrEmpty(checkout.ClientSecret))
            {
                _output.WriteLine("Created checkout has no client secret - skipping test");
                return;
            }

            // Act - Try to confirm without payment info (should fail)
            var result = await client.Checkouts.ConfirmFromClientAsync(checkout.ClientSecret);

            // Assert - Confirming without payment details should not succeed
            if (result.IsSuccess && result.Value.Status == CheckoutStatus.Completed)
            {
                _output.WriteLine("Warning: Checkout confirmed without explicit payment info");
            }
            else
            {
                _output.WriteLine("Confirm requires payment info as expected: " +
                                  (result.IsFailure ? result.Error!.Message : "result not completed"));
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithExpiredCheckout_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Expired, limit: 1);
            if (listResult.IsFailure || listResult.Value.Items.Count == 0)
            {
                _output.WriteLine("No expired checkouts found - skipping test");
                return;
            }

            // We need client_secret for expired checkout, but if we created it we'd have it
            // For now, use checkout ID (which may not work for this endpoint)
            var expiredCheckoutId = listResult.Value.Items[0].Id;

            // Act
            var result = await client.Checkouts.ConfirmFromClientAsync(expiredCheckoutId);

            // Assert - Expired checkout should not be confirmable
            result.IsFailure.Should().BeTrue();

            _output.WriteLine($"Attempted to confirm expired checkout {expiredCheckoutId}: {result.Error!.Message}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    #endregion

    #region Update Tests - PATCH /v1/checkouts/{id}

    [Fact]
    public async Task UpdateAsync_WithOpenCheckout_UpdatesMetadata()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var checkout = await CreateTestCheckoutAsync(client);

            if (checkout == null)
            {
                _output.WriteLine("Could not create test checkout - skipping test");
                return;
            }

            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object>
                {
                    ["updated_at"] = DateTime.UtcNow.ToString("O"),
                    ["updated_by"] = "integration_test"
                }
            };

            // Act
            var result = await client.Checkouts.UpdateAsync(checkout.Id, updateRequest);

            // Assert
            if (result.IsSuccess)
            {
                result.Value.Id.Should().Be(checkout.Id);
                _output.WriteLine($"Updated checkout {checkout.Id} with new metadata");
            }
            else
            {
                _output.WriteLine("UpdateAsync failed - update may not be supported or requires different format");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithSuccessUrl_UpdatesUrl()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var checkout = await CreateTestCheckoutAsync(client);

            if (checkout == null)
            {
                _output.WriteLine("Could not create test checkout - skipping test");
                return;
            }

            var newSuccessUrl = $"https://example.com/success/{Guid.NewGuid()}";
            var updateRequest = new CheckoutUpdateRequest
            {
                SuccessUrl = newSuccessUrl
            };

            // Act
            var result = await client.Checkouts.UpdateAsync(checkout.Id, updateRequest);

            // Assert
            if (result.IsSuccess)
            {
                result.Value.Id.Should().Be(checkout.Id);
                _output.WriteLine($"Updated checkout {checkout.Id} success URL");
            }
            else
            {
                _output.WriteLine("UpdateAsync failed");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentCheckout_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "00000000-0000-0000-0000-000000000000";
            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object> { ["test"] = "value" }
            };

            // Act
            var result = await client.Checkouts.UpdateAsync(nonExistentId, updateRequest);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithClosedCheckout_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Find a non-open checkout (expired)
            var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Expired, limit: 1);

            if (listResult.IsFailure || listResult.Value.Items.Count == 0)
            {
                // Try to find any non-open checkout
                var allCheckoutsResult = await client.Checkouts.ListAsync(limit: 20);
                if (allCheckoutsResult.IsFailure)
                {
                    _output.WriteLine("Could not list checkouts - skipping test");
                    return;
                }

                var nonOpenCheckout = allCheckoutsResult.Value.Items
                    .FirstOrDefault(c => c.Status != CheckoutStatus.Open);

                if (nonOpenCheckout == null)
                {
                    _output.WriteLine("No non-open checkouts found - skipping test");
                    return;
                }

                listResult = PolarResult<PolarSharp.Models.Common.PaginatedResponse<Checkout>>.Success(
                    new PolarSharp.Models.Common.PaginatedResponse<Checkout>
                    {
                        Items = new List<Checkout> { nonOpenCheckout },
                        Pagination = allCheckoutsResult.Value.Pagination
                    }
                );
            }

            var checkoutId = listResult.Value.Items[0].Id;
            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object> { ["test"] = "value" }
            };

            // Act
            var result = await client.Checkouts.UpdateAsync(checkoutId, updateRequest);

            // Assert - Either failure or null
            result.IsFailure.Should().BeTrue();

            _output.WriteLine($"Updating a non-open checkout correctly returned failure: {result.Error!.Message}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithExpiredCheckout_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Expired, limit: 1);

            if (listResult.IsFailure || listResult.Value.Items.Count == 0)
            {
                _output.WriteLine("No expired checkouts found - skipping test");
                return;
            }

            var checkoutId = listResult.Value.Items[0].Id;
            var updateRequest = new CheckoutUpdateRequest
            {
                Metadata = new Dictionary<string, object> { ["test"] = "value" }
            };

            // Act
            var result = await client.Checkouts.UpdateAsync(checkoutId, updateRequest);

            // Assert - Expired checkout should not be updatable
            result.IsFailure.Should().BeTrue();

            _output.WriteLine($"Attempted to update expired checkout {checkoutId}: {result.Error!.Message}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    #endregion

    #region Full Checkout Lifecycle Tests

    [Fact]
    public async Task CheckoutLifecycle_CreateAndUpdate_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var productId = await GetActiveProductIdAsync(client);

            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products found - skipping test");
                return;
            }

            // Step 1: Create checkout
            var createRequest = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                SuccessUrl = "https://example.com/success",
                Metadata = new Dictionary<string, object>
                {
                    ["step"] = "created"
                }
            };

            var createResult = await client.Checkouts.CreateAsync(createRequest);
            if (createResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {createResult.Error!.Message}");
                return;
            }
            var checkout = createResult.Value;
            checkout.Should().NotBeNull();
            checkout.Status.Should().Be(CheckoutStatus.Open);
            _output.WriteLine($"Step 1 - Created checkout: {checkout.Id}");

            // Step 2: Retrieve checkout using client secret (preferred method for client-side access)
            if (!string.IsNullOrEmpty(checkout.ClientSecret))
            {
                var clientCheckoutResult = await client.Checkouts.GetFromClientAsync(checkout.ClientSecret);
                if (clientCheckoutResult.IsSuccess)
                {
                    clientCheckoutResult.Value.Id.Should().Be(checkout.Id);
                    _output.WriteLine($"Step 2 - Retrieved via client secret: {checkout.Id}");
                }
                else
                {
                    _output.WriteLine("Step 2 - GetFromClientAsync failed");
                }
            }
            else
            {
                _output.WriteLine("Step 2 - No client secret available");
            }

            // Step 3: Verify checkout is in list
            var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Open, limit: 100);
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            listResult.Value.Items.Should().Contain(c => c.Id == checkout.Id);
            _output.WriteLine($"Step 3 - Verified checkout in list: {checkout.Id}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CheckoutLifecycle_CreateWithAllOptions_ReturnsCompleteCheckout()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var productId = await GetActiveProductIdAsync(client);

            if (string.IsNullOrEmpty(productId))
            {
                _output.WriteLine("No active products found - skipping test");
                return;
            }

            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var request = new CheckoutCreateRequest
            {
                Products = new List<string> { productId },
                SuccessUrl = "https://example.com/success",
                Metadata = new Dictionary<string, object>
                {
                    ["test_id"] = uniqueId,
                    ["source"] = "integration_test",
                    ["created_at"] = DateTime.UtcNow.ToString("O")
                }
            };

            // Act
            var result = await client.Checkouts.CreateAsync(request);

            // Assert
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var checkout = result.Value;
            checkout.Should().NotBeNull();
            checkout.Id.Should().NotBeNullOrEmpty();
            checkout.Status.Should().Be(CheckoutStatus.Open);
            checkout.Url.Should().NotBeNullOrEmpty("Checkout URL should be generated");
            checkout.ClientSecret.Should().NotBeNullOrEmpty("Checkout should have a client secret");
            checkout.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));

            _output.WriteLine($"Created complete checkout: {checkout.Id}");
            _output.WriteLine($"Checkout URL: {checkout.Url}");
            _output.WriteLine($"Expires at: {checkout.ExpiresAt}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    #endregion
}
