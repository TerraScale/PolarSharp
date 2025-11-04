using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PolarSharp.Models.Checkouts;
using PolarSharp.Models.Common;
using PolarSharp.Models.Products;
using PolarSharp.Exceptions;
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
        result.Items.Should().NotBeNull();
        result.Pagination.Page.Should().BeGreaterThanOrEqualTo(1);
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
        result.Pagination.Page.Should().Be(page);
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            // Act
            var result = await client.CheckoutLinks.ListAsync(productId: product.Id);

            // Assert
            result.Should().NotBeNull();
            // Note: This might return empty if no checkout links exist for this product yet
            if (result.Items.Any())
                result.Items.Should().AllSatisfy(item => item.ProductId.Should().Be(product.Id));
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
        disabledResult.Should().NotBeNull();
        
        if (enabledResult.Items.Any())
            enabledResult.Items.Should().AllSatisfy(item => item.Enabled.Should().BeTrue());
        
        if (disabledResult.Items.Any())
            disabledResult.Items.Should().AllSatisfy(item => item.Enabled.Should().BeFalse());
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
        activeResult.Should().NotBeNull();
        
        if (archivedResult.Items.Any())
            archivedResult.Items.Should().AllSatisfy(item => item.Archived.Should().BeTrue());
        
        if (activeResult.Items.Any())
            activeResult.Items.Should().AllSatisfy(item => item.Archived.Should().BeFalse());
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
        result.Items.Should().AllSatisfy(item =>
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
        await foreach (var checkoutLink in client.CheckoutLinks.ListAllAsync())
        {
            checkoutLinks.Add(checkoutLink);
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
        await foreach (var checkoutLink in client.CheckoutLinks.ListAllAsync(enabled: true))
        {
            checkoutLinks.Add(checkoutLink);
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 2000,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Test Checkout Link for Get",
                Description = "Test checkout link for get operation",
                Enabled = true
            };
            var createdCheckoutLink = await client.CheckoutLinks.CreateAsync(createRequest);

            try
            {
                // Act
                var retrievedCheckoutLink = await client.CheckoutLinks.GetAsync(createdCheckoutLink.Id);

                // Assert
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
                try { await client.CheckoutLinks.DeleteAsync(createdCheckoutLink.Id); } catch { }
            }
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ThrowsPolarApiException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidId = "invalid_checkout_link_id";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PolarApiException>(
            () => client.CheckoutLinks.GetAsync(invalidId));
        
        exception.Message.Should().Contain("404");
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 1500,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

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
                result.Should().NotBeNull();
                result.Id.Should().NotBeEmpty();
                result.Url.Should().NotBeEmpty();
                result.Label.Should().Be(createRequest.Label);
                result.Description.Should().Be(createRequest.Description);
                result.ProductId.Should().Be(createRequest.ProductId);
                result.ProductPriceId.Should().Be(createRequest.ProductPriceId);
                result.Enabled.Should().Be(createRequest.Enabled);
                result.Metadata.Should().NotBeNull();
                result.Metadata["test_key"].Should().Be("test_value");
                result.Metadata["integration_test"].Should().Be(true);
                result.Archived.Should().BeFalse();
                result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
                result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            }
            finally
            {
                // Cleanup
                try { await client.CheckoutLinks.DeleteAsync(result.Id); } catch { }
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 1000,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

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
                result.Should().NotBeNull();
                result.Id.Should().NotBeEmpty();
                result.Url.Should().NotBeEmpty();
                result.ProductId.Should().Be(createRequest.ProductId);
                result.ProductPriceId.Should().Be(createRequest.ProductPriceId);
                result.Enabled.Should().BeTrue(); // Default value
                result.Label.Should().BeNull();
                result.Description.Should().BeNull();
            }
            finally
            {
                // Cleanup
                try { await client.CheckoutLinks.DeleteAsync(result.Id); } catch { }
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 3000,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Original Label",
                Description = "Original Description",
                Enabled = true
            };
            var createdCheckoutLink = await client.CheckoutLinks.CreateAsync(createRequest);

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
                result.Should().NotBeNull();
                result.Id.Should().Be(createdCheckoutLink.Id);
                result.Label.Should().Be(updateRequest.Label);
                result.Description.Should().Be(updateRequest.Description);
                result.Enabled.Should().Be(updateRequest.Enabled!.Value);
                result.Metadata.Should().NotBeNull();
                result.Metadata["updated"].Should().Be(true);
                result.Metadata["version"].Should().Be(2);
                result.UpdatedAt.Should().BeAfter(createdCheckoutLink.UpdatedAt);
            }
            finally
            {
                // Cleanup checkout link
                try { await client.CheckoutLinks.DeleteAsync(createdCheckoutLink.Id); } catch { }
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 2500,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Original Label",
                Description = "Original Description",
                Enabled = true
            };
            var createdCheckoutLink = await client.CheckoutLinks.CreateAsync(createRequest);

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
                result.Should().NotBeNull();
                result.Id.Should().Be(createdCheckoutLink.Id);
                result.Label.Should().Be(updateRequest.Label);
                result.Description.Should().Be(createdCheckoutLink.Description); // Should remain unchanged
                result.Enabled.Should().Be(createdCheckoutLink.Enabled); // Should remain unchanged
            }
            finally
            {
                // Cleanup checkout link
                try { await client.CheckoutLinks.DeleteAsync(createdCheckoutLink.Id); } catch { }
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 1800,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Checkout Link to Delete"
            };
            var createdCheckoutLink = await client.CheckoutLinks.CreateAsync(createRequest);

            // Act
            var deletedCheckoutLink = await client.CheckoutLinks.DeleteAsync(createdCheckoutLink.Id);

            // Assert
            deletedCheckoutLink.Should().NotBeNull();
            deletedCheckoutLink.Id.Should().Be(createdCheckoutLink.Id);
            deletedCheckoutLink.Label.Should().Be(createdCheckoutLink.Label);
            
            // Verify the checkout link is actually deleted by trying to get it
            await Assert.ThrowsAsync<PolarApiException>(
                () => client.CheckoutLinks.GetAsync(createdCheckoutLink.Id));
        }
        finally
        {
            // Cleanup product
            try { await client.Products.ArchiveAsync(product.Id); } catch { }
        }
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsPolarApiException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidId = "invalid_checkout_link_id";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PolarApiException>(
            () => client.CheckoutLinks.DeleteAsync(invalidId));
        
        exception.Message.Should().Contain("404");
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
        var product = await client.Products.CreateAsync(productRequest);

        try
        {
            var priceRequest = new ProductPriceCreateRequest
            {
                Amount = 2200,
                Currency = "USD",
                Type = ProductPriceType.Fixed
            };
            var price = await client.Products.CreatePriceAsync(product.Id, priceRequest);

            // Create a checkout link
            var createRequest = new CheckoutLinkCreateRequest
            {
                ProductId = product.Id,
                ProductPriceId = price.Id,
                Label = "Checkout Link with Nested Objects"
            };
            var createdCheckoutLink = await client.CheckoutLinks.CreateAsync(createRequest);

            try
            {
                // Act
                var retrievedCheckoutLink = await client.CheckoutLinks.GetAsync(createdCheckoutLink.Id);

                // Assert
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
                try { await client.CheckoutLinks.DeleteAsync(createdCheckoutLink.Id); } catch { }
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
        result.Items.Should().AllSatisfy(item => item.CreatedAt.Should().BeOnOrAfter(yesterday));
    }
}