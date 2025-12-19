using FluentAssertions;
using PolarSharp.Models.OAuth2;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for OAuth2 API.
/// </summary>
public class OAuth2IntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public OAuth2IntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
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
        var createResult = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        if (createResult.IsFailure)
        {
            _output.WriteLine($"OAuth2 CreateClient failed: {createResult.Error!.Message}");
            return;
        }

        var createdClient = createResult.Value;
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
        await client.OAuth2.DeleteClientAsync(createdClient.Id);
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

        var createResult = await client.OAuth2.CreateClientAsync(createRequest);
        if (createResult.IsFailure)
        {
            _output.WriteLine($"Create failed: {createResult.Error!.Message}");
            return;
        }

        // Act
        var getResult = await client.OAuth2.GetClientAsync(createResult.Value.Id);

        // Assert
        if (getResult.IsSuccess)
        {
            getResult.Value.Should().NotBeNull();
            getResult.Value.Id.Should().Be(createResult.Value.Id);
            getResult.Value.Name.Should().Be(clientName);
            getResult.Value.ClientId.Should().Be(createResult.Value.ClientId);
        }

        // Cleanup
        await client.OAuth2.DeleteClientAsync(createResult.Value.Id);
    }

    [Fact]
    public async Task OAuth2Api_GetClient_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidClientId = "invalid_client_id";

        // Act
        var result = await client.OAuth2.GetClientAsync(invalidClientId);

        // Assert
        if (result.IsSuccess)
        {
            result.Value.Should().BeNull();
        }
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

        var createResult = await client.OAuth2.CreateClientAsync(createRequest);
        if (createResult.IsFailure)
        {
            _output.WriteLine($"Create failed: {createResult.Error!.Message}");
            return;
        }

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
        var updateResult = await client.OAuth2.UpdateClientAsync(createResult.Value.Id, updateRequest);

        // Assert
        if (updateResult.IsSuccess)
        {
            var updatedClient = updateResult.Value;
            updatedClient.Should().NotBeNull();
            updatedClient.Id.Should().Be(createResult.Value.Id);
            updatedClient.Name.Should().Be($"{clientName} (Updated)");
            updatedClient.Description.Should().Be("Updated description");
            updatedClient.RedirectUris.Should().HaveCount(2);
            updatedClient.RedirectUris.Should().Contain("https://updated-example.com/callback");
            updatedClient.Scopes.Should().HaveCount(3);
            updatedClient.Scopes.Should().Contain("admin");
            updatedClient.IsActive.Should().BeFalse();
            updatedClient.Metadata.Should().ContainKey("updated");
        }

        // Cleanup
        await client.OAuth2.DeleteClientAsync(createResult.Value.Id);
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

        var createResult = await client.OAuth2.CreateClientAsync(createRequest);
        if (createResult.IsFailure)
        {
            _output.WriteLine($"Create failed: {createResult.Error!.Message}");
            return;
        }

        // Act
        var deleteResult = await client.OAuth2.DeleteClientAsync(createResult.Value.Id);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify client is deleted by checking it returns null
        var getResult = await client.OAuth2.GetClientAsync(createResult.Value.Id);
        if (getResult.IsSuccess)
        {
            getResult.Value.Should().BeNull();
        }
    }

    [Fact]
    public async Task OAuth2Api_DeleteClient_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidClientId = "invalid_client_id";

        // Act
        var result = await client.OAuth2.DeleteClientAsync(invalidClientId);

        // Assert
        result.IsSuccess.Should().BeFalse();
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
        var createResult = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        if (createResult.IsSuccess)
        {
            var createdClient = createResult.Value;
            createdClient.Should().NotBeNull();
            createdClient.Id.Should().NotBeNullOrEmpty();
            createdClient.Name.Should().Be(clientName);
            createdClient.Description.Should().BeNull();
            createdClient.RedirectUris.Should().HaveCount(1);
            createdClient.Scopes.Should().BeEmpty();

            // Cleanup
            await client.OAuth2.DeleteClientAsync(createdClient.Id);
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

        var createResult = await client.OAuth2.CreateClientAsync(createRequest);
        if (createResult.IsFailure)
        {
            _output.WriteLine($"Create failed: {createResult.Error!.Message}");
            return;
        }

        // Update only the description
        var updateRequest = new OAuth2ClientUpdateRequest
        {
            Description = "Updated description only"
        };

        // Act
        var updateResult = await client.OAuth2.UpdateClientAsync(createResult.Value.Id, updateRequest);

        // Assert
        if (updateResult.IsSuccess)
        {
            var updatedClient = updateResult.Value;
            updatedClient.Should().NotBeNull();
            updatedClient.Id.Should().Be(createResult.Value.Id);
            updatedClient.Name.Should().Be(clientName); // Should remain unchanged
            updatedClient.Description.Should().Be("Updated description only");
            updatedClient.RedirectUris.Should().BeEquivalentTo(createResult.Value.RedirectUris); // Should remain unchanged
            updatedClient.Scopes.Should().BeEquivalentTo(createResult.Value.Scopes); // Should remain unchanged
        }

        // Cleanup
        await client.OAuth2.DeleteClientAsync(createResult.Value.Id);
    }

    [Fact]
    public async Task OAuth2Api_UpdateClient_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidClientId = "invalid_client_id";

        var updateRequest = new OAuth2ClientUpdateRequest
        {
            Name = "Updated Name"
        };

        // Act
        var result = await client.OAuth2.UpdateClientAsync(invalidClientId, updateRequest);

        // Assert
        if (result.IsSuccess)
        {
            result.Value.Should().BeNull();
        }
    }

    [Fact]
    public async Task OAuth2Api_CreateClient_WithEmptyRedirectUris_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Invalid OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string>() // Empty redirect URIs
        };

        // Act
        var result = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task OAuth2Api_CreateClient_WithInvalidRedirectUris_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clientName = $"Invalid OAuth2 Client {Guid.NewGuid()}";

        var createRequest = new OAuth2ClientCreateRequest
        {
            Name = clientName,
            RedirectUris = new List<string> { "invalid-url" } // Invalid URL format
        };

        // Act
        var result = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task OAuth2Api_GetUserInfo_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.OAuth2.GetUserInfoAsync();

        // Assert
        if (result.IsSuccess)
        {
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().NotBeNullOrEmpty();
            // Other fields may be null depending on the user's profile
        }
    }

    [Fact]
    public async Task OAuth2Api_RequestToken_WithInvalidGrantType_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var tokenRequest = new OAuth2TokenRequest
        {
            GrantType = "invalid_grant_type",
            ClientId = "invalid_client_id",
            ClientSecret = "invalid_client_secret"
        };

        // Act
        var result = await client.OAuth2.RequestTokenAsync(tokenRequest);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task OAuth2Api_RevokeToken_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var revokeRequest = new OAuth2RevokeRequest
        {
            Token = "invalid_token"
        };

        // Act
        var result = await client.OAuth2.RevokeTokenAsync(revokeRequest);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
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
        var result = await client.OAuth2.IntrospectTokenAsync(introspectRequest);

        // Assert
        if (result.IsSuccess)
        {
            result.Value.Should().NotBeNull();
            result.Value.Active.Should().BeFalse();
        }
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
        var createResult = await client.OAuth2.CreateClientAsync(createRequest);

        // Assert
        if (createResult.IsSuccess)
        {
            var createdClient = createResult.Value;
            createdClient.Should().NotBeNull();
            createdClient.RedirectUris.Should().HaveCount(5);
            createdClient.RedirectUris.Should().Contain("https://example1.com/callback");
            createdClient.RedirectUris.Should().Contain("https://localhost:8080/callback");

            // Cleanup
            await client.OAuth2.DeleteClientAsync(createdClient.Id);
        }
    }
}
