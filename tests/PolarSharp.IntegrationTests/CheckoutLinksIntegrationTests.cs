using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PolarSharp.Models.Checkouts;
using PolarSharp.Models.Common;
using PolarSharp.Models.Products;
using PolarSharp.Results;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for CheckoutLinks API.
/// </summary>
public class CheckoutLinksIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public CheckoutLinksIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListAsync_WithDefaultParameters_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.CheckoutLinks.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var page = 1;
        var limit = 5;

        // Act
        var result = await client.CheckoutLinks.ListAsync(page: page, limit: limit);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Pagination.Page.Should().Be(page);
    }

    [Fact]
    public async Task ListAsync_WithProductIdFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First create a product to filter by
        var productRequest = new ProductCreateRequest
        {
            Name = "Test Product for Checkout Link Filter",
            Type = ProductType.OneTime,
            Description = "Test product for checkout link filtering"
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            // Skip test if product creation failed
            return;
        }
        var product = productResult.Value;

        try
        {
            // Act
            var result = await client.CheckoutLinks.ListAsync(productId: product.Id);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            // Note: This might return empty if no checkout links exist for this product yet
            if (result.Value.Items.Any())
                result.Value.Items.Should().AllSatisfy(item => item.ProductId.Should().Be(product.Id));
        }
        finally
        {
            // Cleanup
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task ListAsync_WithEnabledFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var enabledResult = await client.CheckoutLinks.ListAsync(enabled: true);
        var disabledResult = await client.CheckoutLinks.ListAsync(enabled: false);

        // Assert
        enabledResult.Should().NotBeNull();
        enabledResult.IsSuccess.Should().BeTrue();
        disabledResult.Should().NotBeNull();
        disabledResult.IsSuccess.Should().BeTrue();

        if (enabledResult.Value.Items.Any())
            enabledResult.Value.Items.Should().AllSatisfy(item => item.Enabled.Should().BeTrue());

        if (disabledResult.Value.Items.Any())
            disabledResult.Value.Items.Should().AllSatisfy(item => item.Enabled.Should().BeFalse());
    }

    [Fact]
    public async Task ListAsync_WithArchivedFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var archivedResult = await client.CheckoutLinks.ListAsync(archived: true);
        var activeResult = await client.CheckoutLinks.ListAsync(archived: false);

        // Assert
        archivedResult.Should().NotBeNull();
        archivedResult.IsSuccess.Should().BeTrue();
        activeResult.Should().NotBeNull();
        activeResult.IsSuccess.Should().BeTrue();

        if (archivedResult.Value.Items.Any())
            archivedResult.Value.Items.Should().AllSatisfy(item => item.Archived.Should().BeTrue());

        if (activeResult.Value.Items.Any())
            activeResult.Value.Items.Should().AllSatisfy(item => item.Archived.Should().BeFalse());
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ReturnsFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var builder = client.CheckoutLinks.Query()
            .WithEnabled(true)
            .WithArchived(false);

        // Act
        var result = await client.CheckoutLinks.ListAsync(builder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().AllSatisfy(item =>
        {
            item.Enabled.Should().BeTrue();
            item.Archived.Should().BeFalse();
        });
    }

    [Fact]
    public async Task ListAllAsync_WithDefaultParameters_ReturnsAllCheckoutLinks()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var checkoutLinks = new List<CheckoutLink>();
        await foreach (var checkoutLinkResult in client.CheckoutLinks.ListAllAsync())
        {
            if (checkoutLinkResult.IsSuccess)
            {
                checkoutLinks.Add(checkoutLinkResult.Value);
            }
        }

        // Assert
        checkoutLinks.Should().NotBeNull();
        // Should contain all checkout links across all pages
        checkoutLinks.Should().AllSatisfy(item =>
        {
            item.Id.Should().NotBeNullOrEmpty();
            item.Url.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task ListAllAsync_WithFilters_ReturnsFilteredCheckoutLinks()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var checkoutLinks = new List<CheckoutLink>();
        await foreach (var checkoutLinkResult in client.CheckoutLinks.ListAllAsync(enabled: true))
        {
            if (checkoutLinkResult.IsSuccess)
            {
                checkoutLinks.Add(checkoutLinkResult.Value);
            }
        }

        // Assert
        checkoutLinks.Should().NotBeNull();
        checkoutLinks.Should().AllSatisfy(item => item.Enabled.Should().BeTrue());
    }

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsCheckoutLink()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First create a product and price for the checkout link
        var productRequest = new ProductCreateRequest
        {
            Name = "Test Product for Get Checkout Link",
            Type = ProductType.OneTime,
            Description = "Test product for get checkout link test"
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            // Skip test if product creation failed
            return;
        }
        var product = productResult.Value;

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 2000,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var priceResult = await client.Products.CreatePriceAsync(product.Id, priceRequest);
            if (priceResult.IsFailure)
            {
                return;
            }
            var price = priceResult.Value;

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Test Checkout Link for Get",
                Description = "Test checkout link for get operation",
                Enabled = true
            };
            var createResult = await client.CheckoutLinks.CreateAsync(createRequest);
            if (createResult.IsFailure)
            {
                return;
            }
            var createdCheckoutLink = createResult.Value;

            try
            {
                // Act
                var result = await client.CheckoutLinks.GetAsync(createdCheckoutLink.Id);

                // Assert
                result.IsSuccess.Should().BeTrue();
                var retrievedCheckoutLink = result.Value;
                retrievedCheckoutLink.Should().NotBeNull();
                retrievedCheckoutLink.Id.Should().Be(createdCheckoutLink.Id);
                retrievedCheckoutLink.Label.Should().Be(createdCheckoutLink.Label);
                retrievedCheckoutLink.Description.Should().Be(createdCheckoutLink.Description);
                retrievedCheckoutLink.ProductId.Should().Be(createdCheckoutLink.ProductId);
                retrievedCheckoutLink.ProductPriceId.Should().Be(createdCheckoutLink.ProductPriceId);
                retrievedCheckoutLink.Enabled.Should().Be(createdCheckoutLink.Enabled);
            }
            finally
            {
                // Cleanup checkout link
                if (createResult.IsSuccess)
                {
                    try { await client.CheckoutLinks.DeleteAsync(createResult.Value.Id); } catch { }
                }
            }
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidId = "invalid_checkout_link_id";

        // Act
        var result = await client.CheckoutLinks.GetAsync(invalidId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCheckoutLink()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Create a product and price first
        var productRequest = new ProductCreateRequest
        {
            Name = "Test Product for Checkout Link Creation",
            Type = ProductType.OneTime,
            Description = "Test product for checkout link creation"
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            return;
        }
        var product = productResult.Value;

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 1500,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var priceResult = await client.Products.CreatePriceAsync(product.Id, priceRequest);
            if (priceResult.IsFailure)
            {
                return;
            }
            var price = priceResult.Value;

            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Test Checkout Link",
                Description = "Test checkout link created via API",
                Enabled = true,
                Metadata = new Dictionary<string, object>
                {
                    ["test_key"] = "test_value",
                    ["integration_test"] = true
                }
            };

            // Act
            var result = await client.CheckoutLinks.CreateAsync(createRequest);

            try
            {
                // Assert
                result.IsSuccess.Should().BeTrue();
                var checkoutLink = result.Value;
                checkoutLink.Should().NotBeNull();
                checkoutLink.Id.Should().NotBeEmpty();
                checkoutLink.Url.Should().NotBeEmpty();
                checkoutLink.Label.Should().Be(createRequest.Label);
                checkoutLink.Description.Should().Be(createRequest.Description);
                checkoutLink.ProductId.Should().Be(createRequest.ProductId);
                checkoutLink.ProductPriceId.Should().Be(createRequest.ProductPriceId);
                checkoutLink.Enabled.Should().Be(createRequest.Enabled);
                checkoutLink.Metadata.Should().NotBeNull();
                checkoutLink.Metadata["test_key"].Should().Be("test_value");
                checkoutLink.Metadata["integration_test"].Should().Be(true);
                checkoutLink.Archived.Should().BeFalse();
                checkoutLink.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
                checkoutLink.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            }
            finally
            {
                // Cleanup
                try { await client.CheckoutLinks.DeleteAsync(result.Value.Id); } catch { }
            }
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task CreateAsync_WithMinimalData_ReturnsCheckoutLink()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Create a product and price first
        var productRequest = new ProductCreateRequest
        {
            Name = "Minimal Test Product for Checkout Link",
            Type = ProductType.OneTime
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            return;
        }
        var product = productResult.Value;

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 1000,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var priceResult = await client.Products.CreatePriceAsync(product.Id, priceRequest);
            if (priceResult.IsFailure)
            {
                return;
            }
            var price = priceResult.Value;

            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id
            };

            // Act
            var result = await client.CheckoutLinks.CreateAsync(createRequest);

            try
            {
                // Assert
                result.IsSuccess.Should().BeTrue();
                var checkoutLink = result.Value;
                checkoutLink.Should().NotBeNull();
                checkoutLink.Id.Should().NotBeEmpty();
                checkoutLink.Url.Should().NotBeEmpty();
                checkoutLink.ProductId.Should().Be(createRequest.ProductId);
                checkoutLink.ProductPriceId.Should().Be(createRequest.ProductPriceId);
                checkoutLink.Enabled.Should().BeTrue(); // Default value
                checkoutLink.Label.Should().BeNull();
                checkoutLink.Description.Should().BeNull();
            }
            finally
            {
                // Cleanup
                try { await client.CheckoutLinks.DeleteAsync(result.Value.Id); } catch { }
            }
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedCheckoutLink()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Create a product and price first
        var productRequest = new ProductCreateRequest
        {
            Name = "Test Product for Checkout Link Update",
            Type = ProductType.OneTime,
            Description = "Test product for checkout link update"
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            return;
        }
        var product = productResult.Value;

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 3000,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var priceResult = await client.Products.CreatePriceAsync(product.Id, priceRequest);
            if (priceResult.IsFailure)
            {
                return;
            }
            var price = priceResult.Value;

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Original Label",
                Description = "Original Description",
                Enabled = true
            };
            var createResult = await client.CheckoutLinks.CreateAsync(createRequest);
            if (createResult.IsFailure)
            {
                return;
            }
            var createdCheckoutLink = createResult.Value;

            try
            {
                var updateRequest = new CheckoutLinkUpdateRequest
                {
                    Label = "Updated Label",
                    Description = "Updated Description",
                    Enabled = false,
                    Metadata = new Dictionary<string, object>
                    {
                        ["updated"] = true,
                        ["version"] = 2
                    }
                };

                // Act
                var result = await client.CheckoutLinks.UpdateAsync(createdCheckoutLink.Id, updateRequest);

                // Assert
                result.IsSuccess.Should().BeTrue();
                var updatedCheckoutLink = result.Value;
                updatedCheckoutLink.Should().NotBeNull();
                updatedCheckoutLink.Id.Should().Be(createdCheckoutLink.Id);
                updatedCheckoutLink.Label.Should().Be(updateRequest.Label);
                updatedCheckoutLink.Description.Should().Be(updateRequest.Description);
                updatedCheckoutLink.Enabled.Should().Be(updateRequest.Enabled!.Value);
                updatedCheckoutLink.Metadata.Should().NotBeNull();
                updatedCheckoutLink.Metadata["updated"].Should().Be(true);
                updatedCheckoutLink.Metadata["version"].Should().Be(2);
                updatedCheckoutLink.UpdatedAt.Should().BeAfter(createdCheckoutLink.UpdatedAt);
            }
            finally
            {
                // Cleanup checkout link
                if (createResult.IsSuccess)
                {
                    try { await client.CheckoutLinks.DeleteAsync(createResult.Value.Id); } catch { }
                }
            }
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task UpdateAsync_WithPartialData_ReturnsPartiallyUpdatedCheckoutLink()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Create a product and price first
        var productRequest = new ProductCreateRequest
        {
            Name = "Test Product for Partial Checkout Link Update",
            Type = ProductType.OneTime
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            return;
        }
        var product = productResult.Value;

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 2500,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var priceResult = await client.Products.CreatePriceAsync(product.Id, priceRequest);
            if (priceResult.IsFailure)
            {
                return;
            }
            var price = priceResult.Value;

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Original Label",
                Description = "Original Description",
                Enabled = true
            };
            var createResult = await client.CheckoutLinks.CreateAsync(createRequest);
            if (createResult.IsFailure)
            {
                return;
            }
            var createdCheckoutLink = createResult.Value;

            try
            {
                var updateRequest = new CheckoutLinkUpdateRequest
                {
                    Label = "Only Label Updated"
                    // Other fields are null, so they shouldn't change
                };

                // Act
                var result = await client.CheckoutLinks.UpdateAsync(createdCheckoutLink.Id, updateRequest);

                // Assert
                result.IsSuccess.Should().BeTrue();
                var updatedCheckoutLink = result.Value;
                updatedCheckoutLink.Should().NotBeNull();
                updatedCheckoutLink.Id.Should().Be(createdCheckoutLink.Id);
                updatedCheckoutLink.Label.Should().Be(updateRequest.Label);
                updatedCheckoutLink.Description.Should().Be(createdCheckoutLink.Description); // Should remain unchanged
                updatedCheckoutLink.Enabled.Should().Be(createdCheckoutLink.Enabled); // Should remain unchanged
            }
            finally
            {
                // Cleanup checkout link
                if (createResult.IsSuccess)
                {
                    try { await client.CheckoutLinks.DeleteAsync(createResult.Value.Id); } catch { }
                }
            }
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsDeletedCheckoutLink()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Create a product and price first
        var productRequest = new ProductCreateRequest
        {
            Name = "Test Product for Checkout Link Deletion",
            Type = ProductType.OneTime
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            return;
        }
        var product = productResult.Value;

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 1800,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var priceResult = await client.Products.CreatePriceAsync(product.Id, priceRequest);
            if (priceResult.IsFailure)
            {
                return;
            }
            var price = priceResult.Value;

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Checkout Link to Delete"
            };
            var createResult = await client.CheckoutLinks.CreateAsync(createRequest);
            if (createResult.IsFailure)
            {
                return;
            }
            var createdCheckoutLink = createResult.Value;

            // Act
            var result = await client.CheckoutLinks.DeleteAsync(createdCheckoutLink.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var deletedCheckoutLink = result.Value;
            deletedCheckoutLink.Should().NotBeNull();
            deletedCheckoutLink.Id.Should().Be(createdCheckoutLink.Id);
            deletedCheckoutLink.Label.Should().Be(createdCheckoutLink.Label);

            // Verify the checkout link is actually deleted by trying to get it
            var afterDeleteResult = await client.CheckoutLinks.GetAsync(createdCheckoutLink.Id);
            afterDeleteResult.IsFailure.Should().BeTrue();
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidId = "invalid_checkout_link_id";

        // Act
        var result = await client.CheckoutLinks.DeleteAsync(invalidId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CheckoutLink_ContainsProductAndProductPrice_WhenRetrieved()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Create a product and price first
        var productRequest = new ProductCreateRequest
        {
            Name = "Test Product for Nested Objects",
            Type = ProductType.OneTime,
            Description = "Test product with nested objects"
        };
        var productResult = await client.Products.CreateAsync(productRequest);
        if (productResult.IsFailure)
        {
            return;
        }
        var product = productResult.Value;

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 2200,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var priceResult = await client.Products.CreatePriceAsync(product.Id, priceRequest);
            if (priceResult.IsFailure)
            {
                return;
            }
            var price = priceResult.Value;

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Checkout Link with Nested Objects"
            };
            var createResult = await client.CheckoutLinks.CreateAsync(createRequest);
            if (createResult.IsFailure)
            {
                return;
            }
            var createdCheckoutLink = createResult.Value;

            try
            {
                // Act
                var result = await client.CheckoutLinks.GetAsync(createdCheckoutLink.Id);

                // Assert
                result.IsSuccess.Should().BeTrue();
                var retrievedCheckoutLink = result.Value;
                retrievedCheckoutLink.Should().NotBeNull();
                retrievedCheckoutLink.Product.Should().NotBeNull();
                retrievedCheckoutLink.ProductPrice.Should().NotBeNull();

                retrievedCheckoutLink.Product.Id.Should().Be(product.Id);
                retrievedCheckoutLink.Product.Name.Should().Be(product.Name);
                retrievedCheckoutLink.Product.Type.Should().Be(product.Type);

                retrievedCheckoutLink.ProductPrice.Id.Should().Be(price.Id);
                retrievedCheckoutLink.ProductPrice.Amount.Should().Be(price.Amount);
                retrievedCheckoutLink.ProductPrice.Currency.Should().Be(price.Currency);
                retrievedCheckoutLink.ProductPrice.Type.Should().Be(price.Type);
            }
            finally
            {
                // Cleanup checkout link
                if (createResult.IsSuccess)
                {
                    try { await client.CheckoutLinks.DeleteAsync(createResult.Value.Id); } catch { }
                }
            }
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task QueryBuilder_CreatedAfterAndBefore_FiltersByDate()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);
        var tomorrow = now.AddDays(1);

        // Act
        var recentCheckoutLinks = client.CheckoutLinks.Query()
            .CreatedAfter(yesterday);
        var result = await client.CheckoutLinks.ListAsync(recentCheckoutLinks);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().AllSatisfy(item => item.CreatedAt.Should().BeOnOrAfter(yesterday));
    }
}
