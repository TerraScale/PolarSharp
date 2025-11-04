using System.Net;
using FluentAssertions;
using Polar.NET.Exceptions;
using Polar.NET.Models.Webhooks;
using Xunit;

namespace Polar.NET.IntegrationTests;

/// <summary>
/// Integration tests for Webhooks API.
/// </summary>
public class WebhooksIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public WebhooksIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListEndpointsAsync_ShouldReturnWebhookEndpoints()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Webhooks.ListEndpointsAsync();

// Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListAllEndpointsAsync_ShouldReturnAllWebhookEndpoints()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var endpointCount = 0;

        // Act
        await foreach (var endpoint in client.Webhooks.ListAllEndpointsAsync())
        {
            endpointCount++;
            endpoint.Should().NotBeNull();
            endpoint.Id.Should().NotBeNullOrEmpty();
            endpoint.Url.Should().NotBeNullOrEmpty();
            endpoint.Events.Should().NotBeNull();
        }

        // Assert
        endpointCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateEndpointAsync_ShouldCreateWebhookEndpoint()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new WebhookEndpointCreateRequest
        {
            Url = "https://example.com/webhook/test",
            Description = "Test webhook endpoint created via integration test",
            Events = new[] { "order.created", "order.updated" },
            IsActive = true,
            HttpMethod = "POST",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer test-token",
                ["X-Custom-Header"] = "test-value"
            }
        };

        // Act
        var endpoint = await client.Webhooks.CreateEndpointAsync(request);

        // Assert
        endpoint.Should().NotBeNull();
        endpoint.Id.Should().NotBeNullOrEmpty();
        endpoint.Url.Should().Be(request.Url);
        endpoint.Description.Should().Be(request.Description);
        endpoint.Events.Should().BeEquivalentTo(request.Events);
        endpoint.IsActive.Should().Be(request.IsActive);
        endpoint.HttpMethod.Should().Be(request.HttpMethod);
        endpoint.Headers.Should().ContainKey("Authorization");
        endpoint.Headers["Authorization"].Should().Be("Bearer test-token");
        endpoint.Secret.Should().NotBeNullOrEmpty(); // Should be auto-generated
    }

    [Fact]
    public async Task GetEndpointAsync_ShouldReturnWebhookEndpoint()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdEndpoint = await CreateTestWebhookEndpointAsync(client);

        // Act
        var retrievedEndpoint = await client.Webhooks.GetEndpointAsync(createdEndpoint.Id);

        // Assert
        retrievedEndpoint.Should().NotBeNull();
        retrievedEndpoint.Id.Should().Be(createdEndpoint.Id);
        retrievedEndpoint.Url.Should().Be(createdEndpoint.Url);
        retrievedEndpoint.Events.Should().BeEquivalentTo(createdEndpoint.Events);
        retrievedEndpoint.IsActive.Should().Be(createdEndpoint.IsActive);
    }

    [Fact]
    public async Task UpdateEndpointAsync_ShouldUpdateWebhookEndpoint()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdEndpoint = await CreateTestWebhookEndpointAsync(client);
        var updateRequest = new WebhookEndpointUpdateRequest
        {
            Url = "https://updated-example.com/webhook/test",
            Description = "Updated webhook endpoint description",
            Events = new[] { "order.created", "order.updated", "order.deleted" },
            IsActive = false,
            HttpMethod = "PATCH",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer updated-token",
                ["X-Updated-Header"] = "updated-value"
            }
        };

        // Act
        var updatedEndpoint = await client.Webhooks.UpdateEndpointAsync(createdEndpoint.Id, updateRequest);

        // Assert
        updatedEndpoint.Should().NotBeNull();
        updatedEndpoint.Id.Should().Be(createdEndpoint.Id);
        updatedEndpoint.Url.Should().Be(updateRequest.Url);
        updatedEndpoint.Description.Should().Be(updateRequest.Description);
        updatedEndpoint.Events.Should().BeEquivalentTo(updateRequest.Events);
        updatedEndpoint.IsActive.Should().Be(updateRequest.IsActive.Value);
        updatedEndpoint.HttpMethod.Should().Be(updateRequest.HttpMethod);
        updatedEndpoint.Headers.Should().ContainKey("Authorization");
        updatedEndpoint.Headers["Authorization"].Should().Be("Bearer updated-token");
    }

    [Fact]
    public async Task ResetEndpointSecretAsync_ShouldGenerateNewSecret()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdEndpoint = await CreateTestWebhookEndpointAsync(client);
        var originalSecret = createdEndpoint.Secret;

        // Act
        var updatedEndpoint = await client.Webhooks.ResetEndpointSecretAsync(createdEndpoint.Id);

        // Assert
        updatedEndpoint.Should().NotBeNull();
        updatedEndpoint.Id.Should().Be(createdEndpoint.Id);
        updatedEndpoint.Secret.Should().NotBeNullOrEmpty();
        updatedEndpoint.Secret.Should().NotBe(originalSecret); // Should be different
    }

    [Fact]
    public async Task DeleteEndpointAsync_ShouldDeleteWebhookEndpoint()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdEndpoint = await CreateTestWebhookEndpointAsync(client);

        // Act
        var deletedEndpoint = await client.Webhooks.DeleteEndpointAsync(createdEndpoint.Id);

        // Assert
        deletedEndpoint.Should().NotBeNull();
        deletedEndpoint.Id.Should().Be(createdEndpoint.Id);
        
        // Verify endpoint is deleted by trying to get it (should fail)
        var exception = await Assert.ThrowsAsync<PolarApiException>(
            () => client.Webhooks.GetEndpointAsync(createdEndpoint.Id));
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ListDeliveriesAsync_ShouldReturnWebhookDeliveries()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Webhooks.ListDeliveriesAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ListDeliveriesAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Webhooks.ListDeliveriesAsync(page: 1, limit: 5);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var builder = client.Webhooks.Query()
            .WithActive(true)
            .WithUrl("example.com");

        // Act
        var response = await client.Webhooks.ListAsync(builder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        
        // Verify filtering (if any endpoints exist)
        foreach (var endpoint in response.Items)
        {
            endpoint.IsActive.Should().BeTrue();
            endpoint.Url.Should().Contain("example.com");
        }
    }

    [Fact]
    public async Task ListAsync_WithDateFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var builder = client.Webhooks.Query()
            .CreatedAfter(yesterday)
            .CreatedBefore(tomorrow);

        // Act
        var response = await client.Webhooks.ListAsync(builder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        
        // Verify date filtering (if any endpoints exist)
        foreach (var endpoint in response.Items)
        {
            endpoint.CreatedAt.Should().BeOnOrAfter(yesterday);
            endpoint.CreatedAt.Should().BeOnOrBefore(tomorrow);
        }
    }

    [Fact]
    public async Task CreateEndpointAsync_WithMinimalData_ShouldCreateWebhookEndpoint()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new WebhookEndpointCreateRequest
        {
            Url = "https://minimal-example.com/webhook",
            Events = new[] { "order.created" }
        };

        // Act
        var endpoint = await client.Webhooks.CreateEndpointAsync(request);

        // Assert
        endpoint.Should().NotBeNull();
        endpoint.Id.Should().NotBeNullOrEmpty();
        endpoint.Url.Should().Be(request.Url);
        endpoint.Events.Should().BeEquivalentTo(request.Events);
        endpoint.Description.Should().BeNull();
        endpoint.IsActive.Should().BeTrue(); // Default value
        endpoint.HttpMethod.Should().Be("POST"); // Default value
        endpoint.Headers.Should().BeEmpty();
        endpoint.Secret.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateEndpointAsync_WithPartialData_ShouldUpdateOnlySpecifiedFields()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdEndpoint = await CreateTestWebhookEndpointAsync(client);
        var originalUrl = createdEndpoint.Url;
        var updateRequest = new WebhookEndpointUpdateRequest
        {
            Description = "Updated description only"
        };

        // Act
        var updatedEndpoint = await client.Webhooks.UpdateEndpointAsync(createdEndpoint.Id, updateRequest);

        // Assert
        updatedEndpoint.Should().NotBeNull();
        updatedEndpoint.Id.Should().Be(createdEndpoint.Id);
        updatedEndpoint.Url.Should().Be(originalUrl); // Should remain unchanged
        updatedEndpoint.Description.Should().Be(updateRequest.Description);
    }

    [Fact]
    public async Task CreateEndpointAsync_WithDifferentHttpMethods_ShouldCreateWebhookEndpoint()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var methods = new[] { "GET", "POST", "PUT", "PATCH" };

        foreach (var method in methods)
        {
            var request = new WebhookEndpointCreateRequest
            {
                Url = $"https://example.com/webhook/{method.ToLower()}",
                Events = new[] { "order.created" },
                HttpMethod = method
            };

            // Act
            var endpoint = await client.Webhooks.CreateEndpointAsync(request);

            // Assert
            endpoint.Should().NotBeNull();
            endpoint.HttpMethod.Should().Be(method);
            
            // Cleanup
            await client.Webhooks.DeleteEndpointAsync(endpoint.Id);
        }
    }

    [Fact]
    public async Task CreateEndpointAsync_WithComplexHeaders_ShouldCreateWebhookEndpoint()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new WebhookEndpointCreateRequest
        {
            Url = "https://example.com/webhook/headers",
            Events = new[] { "order.created" },
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer complex-token-123",
                ["Content-Type"] = "application/json",
                ["X-Webhook-Source"] = "polar-integration-tests",
                ["X-Custom-ID"] = Guid.NewGuid().ToString(),
                ["User-Agent"] = "Polar.NET/1.0"
            }
        };

        // Act
        var endpoint = await client.Webhooks.CreateEndpointAsync(request);

        // Assert
        endpoint.Should().NotBeNull();
        endpoint.Headers.Should().HaveCount(request.Headers.Count);
        foreach (var header in request.Headers)
        {
            endpoint.Headers.Should().ContainKey(header.Key);
            endpoint.Headers[header.Key].Should().Be(header.Value);
        }
    }

    private async Task<WebhookEndpoint> CreateTestWebhookEndpointAsync(PolarClient client)
    {
        var request = new WebhookEndpointCreateRequest
        {
            Url = $"https://webhook.site/{Guid.NewGuid()}",
            Description = "Test webhook endpoint for integration tests",
            Events = new[] { "order.created" },
            IsActive = true,
            HttpMethod = "POST"
        };

        return await client.Webhooks.CreateEndpointAsync(request);
    }
}