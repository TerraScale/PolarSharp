using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.OAuth2;

/// <summary>
/// Represents an OAuth2 authorization request.
/// </summary>
public record OAuth2AuthorizeRequest
{
    /// <summary>
    /// The client ID.
    /// </summary>
    [Required]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// The response type (always "code").
    /// </summary>
    [Required]
    public string ResponseType { get; init; } = "code";

    /// <summary>
    /// The redirect URI.
    /// </summary>
    [Required]
    public string RedirectUri { get; init; } = string.Empty;

    /// <summary>
    /// The requested scopes.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    /// The state parameter for CSRF protection.
    /// </summary>
    public string? State { get; init; }
}

/// <summary>
/// Represents an OAuth2 token request.
/// </summary>
public record OAuth2TokenRequest
{
    /// <summary>
    /// The grant type.
    /// </summary>
    [Required]
    public string GrantType { get; init; } = string.Empty;

    /// <summary>
    /// The client ID.
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// The client secret.
    /// </summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    /// The authorization code.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// The redirect URI.
    /// </summary>
    public string? RedirectUri { get; init; }

    /// <summary>
    /// The refresh token.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// The requested scopes.
    /// </summary>
    public string? Scope { get; init; }
}

/// <summary>
/// Represents an OAuth2 token response.
/// </summary>
public record OAuth2TokenResponse
{
    /// <summary>
    /// The access token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// The token type (always "Bearer").
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;

    /// <summary>
    /// The expires in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>
    /// The refresh token.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>
    /// The granted scopes.
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }
}

/// <summary>
/// Represents an OAuth2 token revoke request.
/// </summary>
public record OAuth2RevokeRequest
{
    /// <summary>
    /// The token to revoke.
    /// </summary>
    [Required]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// The token type hint.
    /// </summary>
    public string? TokenTypeHint { get; init; }
}

/// <summary>
/// Represents an OAuth2 token introspection request.
/// </summary>
public record OAuth2IntrospectRequest
{
    /// <summary>
    /// The token to introspect.
    /// </summary>
    [Required]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// The token type hint.
    /// </summary>
    public string? TokenTypeHint { get; init; }
}

/// <summary>
/// Represents an OAuth2 token introspection response.
/// </summary>
public record OAuth2IntrospectResponse
{
    /// <summary>
    /// Whether the token is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; init; }

    /// <summary>
    /// The client ID.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; init; }

    /// <summary>
    /// The username/user ID.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>
    /// The token scopes.
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    /// <summary>
    /// The token type.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }

    /// <summary>
    /// The expiration time.
    /// </summary>
    [JsonPropertyName("exp")]
    public long? Exp { get; init; }

    /// <summary>
    /// The issued at time.
    /// </summary>
    [JsonPropertyName("iat")]
    public long? Iat { get; init; }

    /// <summary>
    /// The not before time.
    /// </summary>
    [JsonPropertyName("nbf")]
    public long? Nbf { get; init; }

    /// <summary>
    /// The subject.
    /// </summary>
    [JsonPropertyName("sub")]
    public string? Sub { get; init; }

    /// <summary>
    /// The audience.
    /// </summary>
    [JsonPropertyName("aud")]
    public string? Aud { get; init; }

    /// <summary>
    /// The issuer.
    /// </summary>
    [JsonPropertyName("iss")]
    public string? Iss { get; init; }

    /// <summary>
    /// The token ID.
    /// </summary>
    [JsonPropertyName("jti")]
    public string? Jti { get; init; }
}

/// <summary>
/// Represents OAuth2 user info.
/// </summary>
public record OAuth2UserInfo
{
    /// <summary>
    /// The user ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>
    /// The name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The avatar URL.
    /// </summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The username.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>
    /// The GitHub username.
    /// </summary>
    [JsonPropertyName("github_username")]
    public string? GithubUsername { get; init; }
}

/// <summary>
/// Represents an OAuth2 client.
/// </summary>
public record OAuth2Client
{
    /// <summary>
    /// The unique identifier of the client.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The client name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The client description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The client ID.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// The client secret.
    /// </summary>
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; init; }

    /// <summary>
    /// The redirect URIs.
    /// </summary>
    [JsonPropertyName("redirect_uris")]
    public List<string> RedirectUris { get; init; } = new();

    /// <summary>
    /// The allowed scopes.
    /// </summary>
    [JsonPropertyName("scopes")]
    public List<string> Scopes { get; init; } = new();

    /// <summary>
    /// Whether the client is active.
    /// </summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    /// <summary>
    /// The organization ID that owns the client.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The creation date of the client.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the client.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The metadata associated with the client.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to create a new OAuth2 client.
/// </summary>
public record OAuth2ClientCreateRequest
{
    /// <summary>
    /// The client name.
    /// </summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The client description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The redirect URIs.
    /// </summary>
    [Required]
    public List<string> RedirectUris { get; init; } = new();

    /// <summary>
    /// The allowed scopes.
    /// </summary>
    public List<string>? Scopes { get; init; }

    /// <summary>
    /// The metadata to associate with the client.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to update an existing OAuth2 client.
/// </summary>
public record OAuth2ClientUpdateRequest
{
    /// <summary>
    /// The client name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The client description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The redirect URIs.
    /// </summary>
    public List<string>? RedirectUris { get; init; }

    /// <summary>
    /// The allowed scopes.
    /// </summary>
    public List<string>? Scopes { get; init; }

    /// <summary>
    /// Whether the client is active.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// The metadata to associate with the client.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}