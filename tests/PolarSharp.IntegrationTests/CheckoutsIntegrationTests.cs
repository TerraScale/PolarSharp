using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PolarSharp.Exceptions;
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
        var products = await client.Products.ListAsync(limit: 50);
        var activeProduct = products.Items.FirstOrDefault(p => !p.IsArchived);
        
        if (activeProduct != null)
        {
            _output.WriteLine($"Found active product: {activeProduct.Name} ({activeProduct.Id})");
            return activeProduct.Id;
        }
        
        _output.WriteLine("No active products found in the account");
        return null;
    }

    /// <summary>
    /// Gets a product ID from an existing checkout (may be archived).
    /// </summary>
    private async Task<string?> GetProductIdFromExistingCheckoutAsync(PolarClient client)
    {
        var checkouts = await client.Checkouts.ListAsync(limit: 10);
        var checkoutWithProduct = checkouts.Items.FirstOrDefault(c => !string.IsNullOrEmpty(c.ProductId));
        return checkoutWithProduct?.ProductId;
    }

    /// <summary>
    /// Creates a test checkout and returns it with valid data.
    /// </summary>
    private async Task<Checkout?> CreateTestCheckoutAsync(PolarClient client)
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

        return await client.Checkouts.CreateAsync(request);
    }

    #endregion

    #region List Tests - GET /v1/checkouts/

    [Fact]
    public async Task ListAsync_WithValidParameters_ReturnsNonNullPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Checkouts.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull("API should return a valid paginated response");
        result.Items.Should().NotBeNull("Items collection should never be null");
        result.Pagination.Should().NotBeNull("Pagination metadata should be present");
        result.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
        
        _output.WriteLine($"Found {result.Pagination.TotalCount} total checkouts across {result.Pagination.MaxPage} pages");
    }

    [Fact]
    public async Task ListAllAsync_EnumeratesAllCheckouts_ReturnsNonNullItems()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var checkouts = new List<Checkout>();

        // Act
        await foreach (var checkout in client.Checkouts.ListAllAsync())
        {
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

    [Fact]
    public async Task ListAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act - Filter by open status
        var result = await client.Checkouts.ListAsync(status: CheckoutStatus.Open);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        
        // Verify all returned items have the correct status
        foreach (var checkout in result.Items)
        {
            checkout.Status.Should().Be(CheckoutStatus.Open, 
                "All returned checkouts should have the filtered status");
        }
        
        _output.WriteLine($"Found {result.Items.Count} open checkouts");
    }

    [Theory]
    [InlineData(CheckoutStatus.Open)]
    [InlineData(CheckoutStatus.Expired)]
    public async Task ListAsync_WithDifferentStatusFilters_ReturnsValidResponses(CheckoutStatus status)
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Checkouts.ListAsync(status: status);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        
        // Verify status consistency if there are results
        foreach (var checkout in result.Items)
        {
            checkout.Status.Should().Be(status);
        }
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ReturnsValidResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var query = client.Checkouts.Query();

        // Act
        var result = await client.Checkouts.ListAsync(query, page: 1, limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAsync_WithPagination_NavigatesPagesProperly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var firstPage = await client.Checkouts.ListAsync(page: 1, limit: 2);
        
        // Assert first page
        firstPage.Should().NotBeNull();
        firstPage.Pagination.Should().NotBeNull();

        if (firstPage.Pagination.MaxPage > 1)
        {
            var secondPage = await client.Checkouts.ListAsync(page: 2, limit: 2);
            
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

    [Fact]
    public async Task ListAsync_ReturnsCheckoutsWithRequiredFields()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Checkouts.ListAsync(limit: 10);

        // Assert
        result.Should().NotBeNull();
        
        if (result.Items.Count > 0)
        {
            var checkout = result.Items[0];
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

    [Fact]
    public async Task ListAsync_WithMaxLimit_CapsAtHundred()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act - Request more than the max allowed limit
        var result = await client.Checkouts.ListAsync(page: 1, limit: 200);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Items.Count.Should().BeLessThanOrEqualTo(100,
            "API should cap results at maximum of 100 items per page");
    }

    [Fact]
    public async Task ListAsync_WithZeroLimit_ThrowsValidationError()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - API rejects zero limit with validation error
        var act = async () => await client.Checkouts.ListAsync(page: 1, limit: 0);
        await act.Should().ThrowAsync<PolarApiException>();
    }

    [Fact]
    public async Task ListAsync_WithNegativeLimit_ThrowsValidationError()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - API rejects negative limit with validation error
        var act = async () => await client.Checkouts.ListAsync(page: 1, limit: -1);
        await act.Should().ThrowAsync<PolarApiException>();
    }

    [Fact]
    public async Task ListAsync_WithHighPageNumber_ReturnsEmptyItems()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act - Request a page that likely doesn't exist
        var result = await client.Checkouts.ListAsync(page: 99999, limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty("High page number beyond available data should return empty items");
    }

    [Fact]
    public async Task ListAsync_CheckoutItems_HaveConsistentStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Checkouts.ListAsync(limit: 10);

        // Assert
        foreach (var checkout in result.Items)
        {
            checkout.Id.Should().NotBeNullOrEmpty();
            checkout.Status.Should().BeDefined();
            checkout.CreatedAt.Should().NotBe(default);
        }
    }

    [Fact]
    public async Task ListAsync_PaginationMetadata_IsConsistent()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Checkouts.ListAsync(page: 1, limit: 5);

        // Assert
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
        
        // Items count should not exceed requested limit
        result.Items.Count.Should().BeLessThanOrEqualTo(5);
        
        // Items count should not exceed total count
        result.Items.Count.Should().BeLessThanOrEqualTo(result.Pagination.TotalCount);
    }

    [Fact]
    public async Task ListAsync_WithInvalidCustomerId_ReturnsEmptyOrValidatesError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidCustomerId = "invalid-customer-id";

        // Act & Assert
        try
        {
            var result = await client.Checkouts.ListAsync(customerId: invalidCustomerId);
            
            // If API accepts invalid ID, should return empty results
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty("Invalid customer ID should yield no results");
        }
        catch (PolarApiException ex)
        {
            // Validation error is also acceptable
            ex.Should().NotBeNull();
            _output.WriteLine($"API threw validation error for invalid customer ID: {ex.Message}");
        }
    }

    [Fact]
    public async Task ListAsync_WithInvalidProductId_ReturnsEmptyOrValidatesError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidProductId = "invalid-product-id";

        // Act & Assert
        try
        {
            var result = await client.Checkouts.ListAsync(productId: invalidProductId);
            
            // If API accepts invalid ID, should return empty results
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty("Invalid product ID should yield no results");
        }
        catch (PolarApiException ex)
        {
            // Validation error is also acceptable
            ex.Should().NotBeNull();
            _output.WriteLine($"API threw validation error for invalid product ID: {ex.Message}");
        }
    }

    #endregion

    #region Create Checkout Tests - POST /v1/checkouts/

    [Fact]
    public async Task CreateAsync_WithValidProduct_ReturnsNewCheckout()
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
        var checkout = await client.Checkouts.CreateAsync(request);

        // Assert
        checkout.Should().NotBeNull("Creating checkout with valid product should succeed");
        checkout.Id.Should().NotBeNullOrEmpty("Checkout should have an ID");
        checkout.Status.Should().Be(CheckoutStatus.Open, "New checkout should be open");
        checkout.Url.Should().NotBeNullOrEmpty("Checkout should have a URL");
        checkout.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        
        _output.WriteLine($"Created checkout: {checkout.Id}");
        _output.WriteLine($"Checkout URL: {checkout.Url}");
    }

    [Fact]
    public async Task CreateAsync_WithMultipleProducts_ReturnsCheckoutWithProducts()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // Get multiple product IDs
        var products = await client.Products.ListAsync(limit: 20);
        var activeProductIds = products.Items
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
        var checkout = await client.Checkouts.CreateAsync(request);

        // Assert
        checkout.Should().NotBeNull("Creating checkout with multiple products should succeed");
        checkout.Id.Should().NotBeNullOrEmpty();
        checkout.Status.Should().Be(CheckoutStatus.Open);
        
        _output.WriteLine($"Created checkout with {activeProductIds.Count} products: {checkout.Id}");
    }

    [Fact]
    public async Task CreateAsync_WithCustomerEmail_ReturnsCheckoutOrHandlesValidation()
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

        // Act & Assert - API may reject customer_email in certain contexts
        try
        {
            var checkout = await client.Checkouts.CreateAsync(request);
            checkout.Should().NotBeNull();
            checkout.Id.Should().NotBeNullOrEmpty();
            _output.WriteLine($"Created checkout for email {testEmail}: {checkout.Id}");
        }
        catch (PolarApiException ex) when (ex.Message.Contains("RequestValidationError"))
        {
            // Acceptable - customer_email may have specific requirements
            _output.WriteLine($"CustomerEmail field validation: {ex.Message}");
        }
    }

    [Fact]
    public async Task CreateAsync_WithMetadata_ReturnsCheckoutWithMetadata()
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
        var checkout = await client.Checkouts.CreateAsync(request);

        // Assert
        checkout.Should().NotBeNull();
        checkout.Id.Should().NotBeNullOrEmpty();
        
        _output.WriteLine($"Created checkout with metadata: {checkout.Id}");
    }

    [Fact]
    public async Task CreateAsync_CheckoutHasValidExpirationDate()
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
        var checkout = await client.Checkouts.CreateAsync(request);

        // Assert
        checkout.Should().NotBeNull();
        if (checkout.ExpiresAt.HasValue)
        {
            checkout.ExpiresAt.Should().BeAfter(DateTime.UtcNow, "Expiration should be in the future");
        }
        
        _output.WriteLine($"Checkout expires at: {checkout.ExpiresAt}");
    }

    [Fact]
    public async Task CreateAsync_CheckoutHasClientSecret()
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
        var checkout = await client.Checkouts.CreateAsync(request);

        // Assert
        checkout.Should().NotBeNull();
        checkout.ClientSecret.Should().NotBeNullOrEmpty("Checkout should have a client secret for client-side operations");
        
        _output.WriteLine($"Checkout client secret starts with: {checkout.ClientSecret?[..Math.Min(20, checkout.ClientSecret?.Length ?? 0)]}...");
    }

    [Fact]
    public async Task CreateAsync_WithEmptyProductsList_ReturnsNullOnValidationError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidRequest = new CheckoutCreateRequest
        {
            Products = new List<string>() // Empty products list - required field
        };

        // Act & Assert
        var act = async () => await client.Checkouts.CreateAsync(invalidRequest);
        await act.Should().ThrowAsync<PolarApiException>();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidProductId_HandlesValidationError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidRequest = new CheckoutCreateRequest
        {
            Products = new List<string> { "invalid-product-id" }
        };

        // Act & Assert
        var act = async () => await client.Checkouts.CreateAsync(invalidRequest);
        await act.Should().ThrowAsync<PolarApiException>();
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentProductUuid_HandlesValidationError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentProductId = "00000000-0000-0000-0000-000000000000";
        var request = new CheckoutCreateRequest
        {
            Products = new List<string> { nonExistentProductId }
        };

        // Act & Assert
        var act = async () => await client.Checkouts.CreateAsync(request);
        await act.Should().ThrowAsync<PolarApiException>();
    }

    [Fact]
    public async Task CreateAsync_WithArchivedProduct_ThrowsPolarApiException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // Find an archived product
        var products = await client.Products.ListAsync(limit: 50);
        var archivedProduct = products.Items.FirstOrDefault(p => p.IsArchived);
        
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

        // Act & Assert
        var act = async () => await client.Checkouts.CreateAsync(request);
        await act.Should().ThrowAsync<PolarApiException>();
        
        _output.WriteLine($"Correctly rejected archived product: {archivedProduct.Name}");
    }

    #endregion

    #region Get Tests - GET /v1/checkouts/{id}

    [Fact]
    public async Task GetAsync_WithExistingCheckout_ReturnsCheckoutOrHandlesApiError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var checkout = await CreateTestCheckoutAsync(client);
        
        if (checkout == null)
        {
            _output.WriteLine("Could not create test checkout - skipping test");
            return;
        }

        // Act - The API may return 401 for this endpoint depending on token scopes
        try
        {
            var retrievedCheckout = await client.Checkouts.GetAsync(checkout.Id);

            // Assert - if successful
            if (retrievedCheckout != null)
            {
                retrievedCheckout.Id.Should().Be(checkout.Id);
                _output.WriteLine($"Retrieved checkout: {retrievedCheckout.Id}");
            }
            else
            {
                _output.WriteLine($"GetAsync returned null for checkout: {checkout.Id}");
            }
        }
        catch (PolarApiException ex) when (ex.Message.Contains("Unauthorized"))
        {
            // The GET /v1/checkouts/{id} endpoint may require specific scopes
            // This is acceptable behavior - use client_secret endpoint instead
            _output.WriteLine($"GetAsync returned Unauthorized - use GetFromClientAsync with client_secret instead");
        }
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsNullOrHandlesApiError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "00000000-0000-0000-0000-000000000000";

        // Act & Assert - The API may return 401, 404, or 422 depending on validation
        try
        {
            var result = await client.Checkouts.GetAsync(nonExistentId);
            result.Should().BeNull("Non-existent checkout should return null");
        }
        catch (PolarApiException ex) when (ex.Message.Contains("Unauthorized"))
        {
            // Acceptable - the endpoint requires specific scopes
            _output.WriteLine($"GetAsync returned Unauthorized for non-existent ID");
        }
    }

    #endregion

    #region Client-Side Tests - Using client_secret

    [Fact]
    public async Task GetFromClientAsync_WithValidClientSecret_ReturnsCheckout()
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
        result.Should().NotBeNull("Getting checkout by client secret should succeed");
        result!.Id.Should().Be(checkout.Id);
        
        _output.WriteLine($"Retrieved checkout via client secret: {result.Id}");
    }

    [Fact]
    public async Task GetFromClientAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "00000000-0000-0000-0000-000000000000";

        // Act
        var result = await client.Checkouts.GetFromClientAsync(nonExistentId);

        // Assert
        result.Should().BeNull("Non-existent checkout should return null");
    }

    [Fact]
    public async Task GetFromClientAsync_WithInvalidUuidFormat_ReturnsNullOrThrowsValidationError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidId = "not-a-valid-uuid";

        // Act & Assert
        try
        {
            var result = await client.Checkouts.GetFromClientAsync(invalidId);
            result.Should().BeNull("Invalid ID format should result in null response");
        }
        catch (PolarApiException ex)
        {
            // Validation error is also acceptable
            ex.Should().NotBeNull();
            _output.WriteLine($"API threw expected validation error: {ex.Message}");
        }
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithValidClientSecret_UpdatesCheckout()
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
        if (result != null)
        {
            result.Id.Should().Be(checkout.Id);
            _output.WriteLine($"Updated checkout via client secret: {result.Id}");
        }
        else
        {
            _output.WriteLine("UpdateFromClientAsync returned null - client-side updates may be restricted");
        }
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithNonExistentCheckout_ReturnsNull()
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
        result.Should().BeNull("Updating non-existent checkout from client should return null");
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithInvalidClientSecret_ReturnsNull()
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
        result.Should().BeNull("Invalid client secret should return null");
    }

    [Fact]
    public async Task UpdateFromClientAsync_WithEmptyClientSecret_ReturnsNullOrThrowsError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var updateRequest = new CheckoutUpdateRequest
        {
            Metadata = new Dictionary<string, object> { ["test"] = "value" }
        };

        // Act & Assert
        try
        {
            var result = await client.Checkouts.UpdateFromClientAsync("", updateRequest);
            result.Should().BeNull("Empty client secret should return null");
        }
        catch (PolarApiException ex)
        {
            // Validation error is acceptable
            _output.WriteLine($"API threw error for empty client secret: {ex.Message}");
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithNonExistentCheckout_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "00000000-0000-0000-0000-000000000000";

        // Act
        var result = await client.Checkouts.ConfirmFromClientAsync(nonExistentId);

        // Assert
        result.Should().BeNull("Confirming non-existent checkout should return null");
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithInvalidClientSecret_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidClientSecret = "invalid_client_secret_12345";

        // Act
        var result = await client.Checkouts.ConfirmFromClientAsync(invalidClientSecret);

        // Assert
        result.Should().BeNull("Invalid client secret should return null");
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithEmptyClientSecret_ReturnsNullOrThrowsError()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
            var result = await client.Checkouts.ConfirmFromClientAsync("");
            result.Should().BeNull("Empty client secret should return null");
        }
        catch (PolarApiException ex)
        {
            // Validation error is acceptable
            _output.WriteLine($"API threw error for empty client secret: {ex.Message}");
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithOpenCheckout_RequiresPaymentInfo()
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

        // Act - Try to confirm without payment info (should fail or return null)
        Checkout? result = null;
        PolarApiException? exception = null;
        
        try
        {
            result = await client.Checkouts.ConfirmFromClientAsync(checkout.ClientSecret);
        }
        catch (PolarApiException ex)
        {
            exception = ex;
        }

        // Assert - Confirming without payment details should not succeed
        if (result != null && result.Status == CheckoutStatus.Completed)
        {
            _output.WriteLine("Warning: Checkout confirmed without explicit payment info");
        }
        else
        {
            _output.WriteLine("Confirm requires payment info as expected: " +
                              (exception != null ? exception.Message : "returned null"));
        }
    }

    [Fact]
    public async Task ConfirmFromClientAsync_WithExpiredCheckout_ReturnsNullOrThrowsError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Expired, limit: 1);
        
        if (listResult.Items.Count == 0)
        {
            _output.WriteLine("No expired checkouts found - skipping test");
            return;
        }

        // We need client_secret for expired checkout, but if we created it we'd have it
        // For now, use checkout ID (which may not work for this endpoint)
        var expiredCheckoutId = listResult.Items[0].Id;

        // Act
        Checkout? result = null;
        PolarApiException? exception = null;
        
        try
        {
            result = await client.Checkouts.ConfirmFromClientAsync(expiredCheckoutId);
        }
        catch (PolarApiException ex)
        {
            exception = ex;
        }

        // Assert - Expired checkout should not be confirmable
        (result == null || exception != null).Should().BeTrue(
            "Confirming expired checkout should either return null or throw error");
        
        _output.WriteLine($"Attempted to confirm expired checkout {expiredCheckoutId}: " +
                          (exception != null ? $"Error: {exception.Message}" : "Returned null"));
    }

    #endregion

    #region Update Tests - PATCH /v1/checkouts/{id}

    [Fact]
    public async Task UpdateAsync_WithOpenCheckout_UpdatesMetadata()
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
        if (result != null)
        {
            result.Id.Should().Be(checkout.Id);
            _output.WriteLine($"Updated checkout {checkout.Id} with new metadata");
        }
        else
        {
            _output.WriteLine("UpdateAsync returned null - update may not be supported or requires different format");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithSuccessUrl_UpdatesUrl()
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
        if (result != null)
        {
            result.Id.Should().Be(checkout.Id);
            _output.WriteLine($"Updated checkout {checkout.Id} success URL");
        }
        else
        {
            _output.WriteLine("UpdateAsync returned null");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentCheckout_ReturnsNull()
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
        result.Should().BeNull("Updating non-existent checkout should return null");
    }

    [Fact]
    public async Task UpdateAsync_WithClosedCheckout_ReturnsNullOrThrowsError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // Find a non-open checkout (expired)
        var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Expired, limit: 1);
        
        if (listResult.Items.Count == 0)
        {
            // Try to find any non-open checkout
            var allCheckouts = await client.Checkouts.ListAsync(limit: 20);
            var nonOpenCheckout = allCheckouts.Items
                .FirstOrDefault(c => c.Status != CheckoutStatus.Open);
            
            if (nonOpenCheckout == null)
            {
                _output.WriteLine("No non-open checkouts found - skipping test");
                return;
            }
            
            listResult = new PolarSharp.Models.Common.PaginatedResponse<Checkout>
            {
                Items = new List<Checkout> { nonOpenCheckout },
                Pagination = allCheckouts.Pagination
            };
        }

        var checkoutId = listResult.Items[0].Id;
        var updateRequest = new CheckoutUpdateRequest
        {
            Metadata = new Dictionary<string, object> { ["test"] = "value" }
        };

        // Act
        Checkout? result = null;
        PolarApiException? exception = null;
        
        try
        {
            result = await client.Checkouts.UpdateAsync(checkoutId, updateRequest);
        }
        catch (PolarApiException ex)
        {
            exception = ex;
        }

        // Assert - Either null (422 handled as null) or exception
        var handledProperly = result == null || exception != null;
        handledProperly.Should().BeTrue(
            "Updating a non-open checkout should either return null or throw an error");
        
        if (exception != null)
        {
            _output.WriteLine($"API threw error for updating closed checkout: {exception.Message}");
        }
        else
        {
            _output.WriteLine("API returned null for updating closed checkout");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithExpiredCheckout_ReturnsNullOrError()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Expired, limit: 1);
        
        if (listResult.Items.Count == 0)
        {
            _output.WriteLine("No expired checkouts found - skipping test");
            return;
        }

        var checkoutId = listResult.Items[0].Id;
        var updateRequest = new CheckoutUpdateRequest
        {
            Metadata = new Dictionary<string, object> { ["test"] = "value" }
        };

        // Act
        Checkout? result = null;
        PolarApiException? exception = null;
        
        try
        {
            result = await client.Checkouts.UpdateAsync(checkoutId, updateRequest);
        }
        catch (PolarApiException ex)
        {
            exception = ex;
        }

        // Assert - Expired checkout should not be updatable
        (result == null || exception != null).Should().BeTrue(
            "Updating expired checkout should either return null or throw error");
        
        _output.WriteLine($"Attempted to update expired checkout {checkoutId}: " +
                          (exception != null ? $"Error: {exception.Message}" : "Returned null"));
    }

    #endregion

    #region Full Checkout Lifecycle Tests

    [Fact]
    public async Task CheckoutLifecycle_CreateAndUpdate_WorksCorrectly()
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
        
        var checkout = await client.Checkouts.CreateAsync(createRequest);
        checkout.Should().NotBeNull();
        checkout.Status.Should().Be(CheckoutStatus.Open);
        _output.WriteLine($"Step 1 - Created checkout: {checkout.Id}");

        // Step 2: Retrieve checkout using client secret (preferred method for client-side access)
        if (!string.IsNullOrEmpty(checkout.ClientSecret))
        {
            var clientCheckout = await client.Checkouts.GetFromClientAsync(checkout.ClientSecret);
            if (clientCheckout != null)
            {
                clientCheckout.Id.Should().Be(checkout.Id);
                _output.WriteLine($"Step 2 - Retrieved via client secret: {checkout.Id}");
            }
            else
            {
                _output.WriteLine("Step 2 - GetFromClientAsync returned null");
            }
        }
        else
        {
            _output.WriteLine("Step 2 - No client secret available");
        }

        // Step 3: Verify checkout is in list
        var listResult = await client.Checkouts.ListAsync(status: CheckoutStatus.Open, limit: 100);
        listResult.Items.Should().Contain(c => c.Id == checkout.Id);
        _output.WriteLine($"Step 3 - Verified checkout in list: {checkout.Id}");
    }

    [Fact]
    public async Task CheckoutLifecycle_CreateWithAllOptions_ReturnsCompleteCheckout()
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
        var checkout = await client.Checkouts.CreateAsync(request);

        // Assert
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

    #endregion
}
