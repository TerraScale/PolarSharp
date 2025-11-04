using FluentAssertions;
using PolarSharp.Models.OAuth2;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for OAuth2 API.
/// </summary>
public class OAuth2IntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public OAuth2IntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OAuth2Api_CreateClient_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Test OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            Description = "Integration test OAuth2 client",
            RedirectUris = new List<string>
            {
                "https://example.com/callback",
                "https://localhost:3000/callback"
            },
            Scopes = new List<string>
            {
                "read",
                "write"
            },
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["environment"] = "integration"
            }
        };

        // Act
        var createdClient = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        createdClient.Should().NotBeNull();
        createdClient.Id.Should().NotBeNullOrEmpty();
        createdClient.Name.Should().Be(clientName);
        createdClient.Description.Should().Be("Integration test OAuth2 client");
        createdClient.ClientId.Should().NotBeNullOrEmpty();
        createdClient.ClientSecret.Should().NotBeNullOrEmpty();
        createdClient.RedirectUris.Should().HaveCount(2);
        createdClient.RedirectUris.Should().Contain("https://example.com/callback");
        createdClient.RedirectUris.Should().Contain("https://localhost:3000/callback");
        createdClient.Scopes.Should().HaveCount(2);
        createdClient.Scopes.Should().Contain("read");
        createdClient.Scopes.Should().Contain("write");
        createdClient.IsActive.Should().BeTrue();
        createdClient.Metadata.Should().NotBeNull();
        createdClient.Metadata.Should().ContainKey("test");

        // Cleanup
        try
        {
            await client.OAuth2.DeleteClientAsync(createdClient.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OAuth2Api_GetClient_WithValidId_ReturnsClient()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Test OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string> { "https://example.com/callback" }
        };

        var createdClient = await client.OAuth2.CreateClientAsync(createRequest);

        // Act
        var retrievedClient = await client.OAuth2.GetClientAsync(createdClient.Id);

        // Assert
        retrievedClient.Should().NotBeNull();
        retrievedClient.Id.Should().Be(createdClient.Id);
        retrievedClient.Name.Should().Be(clientName);
        retrievedClient.ClientId.Should().Be(createdClient.ClientId);

        // Cleanup
        try
        {
            await client.OAuth2.DeleteClientAsync(createdClient.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OAuth2Api_GetClient_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidClientId = "invalid_client_id";

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.GetClientAsync(invalidClientId));
    }

    [Fact]
    public async Task OAuth2Api_UpdateClient_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Test OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            Description = "Original description",
            RedirectUris = new List<string> { "https://example.com/callback" },
            Scopes = new List<string> { "read" }
        };

        var createdClient = await client.OAuth2.CreateClientAsync(createRequest);

        // Update request
        var updateRequest = new OAuth2ClientUpdateRequest
        {
            Name = $"{clientName} (Updated)",
            Description = "Updated description",
            RedirectUris = new List<string>
            {
                "https://updated-example.com/callback",
                "https://localhost:8080/callback"
            },
            Scopes = new List<string> { "read", "write", "admin" },
            IsActive = false,
            Metadata = new Dictionary<string, object>
            {
                ["updated"] = true,
                ["version"] = 2
            }
        };

        // Act
        var updatedClient = await client.OAuth2.UpdateClientAsync(createdClient.Id, updateRequest);

        // Assert
        updatedClient.Should().NotBeNull();
        updatedClient.Id.Should().Be(createdClient.Id);
        updatedClient.Name.Should().Be($"{clientName} (Updated)");
        updatedClient.Description.Should().Be("Updated description");
        updatedClient.RedirectUris.Should().HaveCount(2);
        updatedClient.RedirectUris.Should().Contain("https://updated-example.com/callback");
        updatedClient.Scopes.Should().HaveCount(3);
        updatedClient.Scopes.Should().Contain("admin");
        updatedClient.IsActive.Should().BeFalse();
        updatedClient.Metadata.Should().ContainKey("updated");

        // Cleanup
        try
        {
            await client.OAuth2.DeleteClientAsync(updatedClient.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OAuth2Api_DeleteClient_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Test OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string> { "https://example.com/callback" }
        };

        var createdClient = await client.OAuth2.CreateClientAsync(createRequest);

        // Act
        await client.OAuth2.DeleteClientAsync(createdClient.Id);

        // Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.GetClientAsync(createdClient.Id));
    }

    [Fact]
    public async Task OAuth2Api_DeleteClient_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidClientId = "invalid_client_id";

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.DeleteClientAsync(invalidClientId));
    }

    [Fact]
    public async Task OAuth2Api_CreateClient_WithMinimalData_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Minimal OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string> { "https://example.com/callback" }
        };

        // Act
        var createdClient = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        createdClient.Should().NotBeNull();
        createdClient.Id.Should().NotBeNullOrEmpty();
        createdClient.Name.Should().Be(clientName);
        createdClient.Description.Should().BeNull();
        createdClient.RedirectUris.Should().HaveCount(1);
        createdClient.Scopes.Should().BeEmpty();

        // Cleanup
        try
        {
            await client.OAuth2.DeleteClientAsync(createdClient.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OAuth2Api_UpdateClient_WithPartialData_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Partial Update OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            Description = "Original description",
            RedirectUris = new List<string> { "https://example.com/callback" },
            Scopes = new List<string> { "read", "write" }
        };

        var createdClient = await client.OAuth2.CreateClientAsync(createRequest);

        // Update only the description
        var updateRequest = new OAuth2ClientUpdateRequest
        {
            Description = "Updated description only"
        };

        // Act
        var updatedClient = await client.OAuth2.UpdateClientAsync(createdClient.Id, updateRequest);

        // Assert
        updatedClient.Should().NotBeNull();
        updatedClient.Id.Should().Be(createdClient.Id);
        updatedClient.Name.Should().Be(clientName); // Should remain unchanged
        updatedClient.Description.Should().Be("Updated description only");
        updatedClient.RedirectUris.Should().BeEquivalentTo(createdClient.RedirectUris); // Should remain unchanged
        updatedClient.Scopes.Should().BeEquivalentTo(createdClient.Scopes); // Should remain unchanged

        // Cleanup
        try
        {
            await client.OAuth2.DeleteClientAsync(updatedClient.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OAuth2Api_UpdateClient_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidClientId = "invalid_client_id";

        var updateRequest = new OAuth2ClientUpdateRequest
        {
            Name = "Updated Name"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.UpdateClientAsync(invalidClientId, updateRequest));
    }

    [Fact]
    public async Task OAuth2Api_CreateClient_WithEmptyRedirectUris_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Invalid OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string>() // Empty redirect URIs
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.CreateClientAsync(createRequest));
    }

    [Fact]
    public async Task OAuth2Api_CreateClient_WithInvalidRedirectUris_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Invalid OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string> { "invalid-url" } // Invalid URL format
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.CreateClientAsync(createRequest));
    }

    [Fact]
    public async Task OAuth2Api_GetUserInfo_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var userInfo = await client.OAuth2.GetUserInfoAsync();

        // Assert
        userInfo.Should().NotBeNull();
        userInfo.Id.Should().NotBeNullOrEmpty();
        // Other fields may be null depending on the user's profile
    }

    [Fact]
    public async Task OAuth2Api_RequestToken_WithInvalidGrantType_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var tokenRequest = new OAuth2TokenRequest
        {
            GrantType = "invalid_grant_type",
            ClientId = "invalid_client_id",
            ClientSecret = "invalid_client_secret"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.RequestTokenAsync(tokenRequest));
    }

    [Fact]
    public async Task OAuth2Api_RevokeToken_WithInvalidToken_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var revokeRequest = new OAuth2RevokeRequest
        {
            Token = "invalid_token"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.OAuth2.RevokeTokenAsync(revokeRequest));
    }

    [Fact]
    public async Task OAuth2Api_IntrospectToken_WithInvalidToken_ReturnsInactive()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var introspectRequest = new OAuth2IntrospectRequest
        {
            Token = "invalid_token"
        };

        // Act
        var response = await client.OAuth2.IntrospectTokenAsync(introspectRequest);

        // Assert
        response.Should().NotBeNull();
        response.Active.Should().BeFalse();
    }

    [Fact]
    public async Task OAuth2Api_CreateClient_WithMultipleRedirectUris_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Multi URI OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string>
            {
                "https://example1.com/callback",
                "https://example2.com/callback",
                "https://example3.com/callback",
                "https://localhost:3000/callback",
                "https://localhost:8080/callback"
            }
        };

        // Act
        var createdClient = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        createdClient.Should().NotBeNull();
        createdClient.RedirectUris.Should().HaveCount(5);
        createdClient.RedirectUris.Should().Contain("https://example1.com/callback");
        createdClient.RedirectUris.Should().Contain("https://localhost:8080/callback");

        // Cleanup
        try
        {
            await client.OAuth2.DeleteClientAsync(createdClient.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}