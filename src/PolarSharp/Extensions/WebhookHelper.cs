using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolarSharp.Extensions;

/// <summary>
/// Helper class for verifying Polar webhook signatures and processing webhook events.
/// </summary>
public static class WebhookHelper
{
    /// <summary>
    /// Verifies a Polar webhook signature.
    /// </summary>
    /// <param name="payload">The raw webhook payload.</param>
    /// <param name="signature">The signature from the X-Polar-Signature header.</param>
    /// <param name="secret">The webhook secret.</param>
    /// <returns>True if the signature is valid, false otherwise.</returns>
    public static bool VerifySignature(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        try
        {
            // Extract timestamp and signature from the header
            // Format: t=timestamp,v1=signature
            var signatureParts = signature.Split(',');
            var timestampPart = signatureParts.FirstOrDefault(p => p.StartsWith("t="));
            var signaturePart = signatureParts.FirstOrDefault(p => p.StartsWith("v1="));

            if (timestampPart == null || signaturePart == null)
            {
                return false;
            }

            var timestamp = timestampPart.Substring(2);
            var expectedSignature = signaturePart.Substring(3);

            // Check timestamp to prevent replay attacks (5 minutes tolerance)
            if (!long.TryParse(timestamp, out var timestampValue))
            {
                return false;
            }

            var webhookTime = DateTimeOffset.FromUnixTimeSeconds(timestampValue);
            var now = DateTimeOffset.UtcNow;
            var timeDifference = Math.Abs((now - webhookTime).TotalSeconds);

            if (timeDifference > 300) // 5 minutes
            {
                return false;
            }

            // Create the signed payload
            var signedPayload = $"{timestamp}.{payload}";

            // Compute HMAC-SHA256
            var key = Encoding.UTF8.GetBytes(secret);
            var message = Encoding.UTF8.GetBytes(signedPayload);

            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(message);
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            // Constant-time comparison to prevent timing attacks
            return SecureCompare(computedSignature, expectedSignature);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parses a webhook payload and returns the event data.
    /// </summary>
    /// <typeparam name="T">The type of event data to deserialize.</typeparam>
    /// <param name="payload">The raw webhook payload.</param>
    /// <param name="options">JSON serializer options.</param>
    /// <returns>The deserialized event data.</returns>
    public static T ParseWebhookEvent<T>(string payload, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrEmpty(payload))
        {
            throw new ArgumentException("Payload cannot be null or empty.", nameof(payload));
        }

        return JsonSerializer.Deserialize<T>(payload, options) 
               ?? throw new InvalidOperationException("Failed to deserialize webhook payload.");
    }

    /// <summary>
    /// Extracts the event type from a webhook payload.
    /// </summary>
    /// <param name="payload">The raw webhook payload.</param>
    /// <returns>The event type if found, null otherwise.</returns>
    public static string? GetEventType(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("type", out var typeElement))
            {
                return typeElement.GetString();
            }
        }
        catch
        {
            // Invalid JSON
        }

        return null;
    }

    /// <summary>
    /// Extracts the event ID from a webhook payload.
    /// </summary>
    /// <param name="payload">The raw webhook payload.</param>
    /// <returns>The event ID if found, null otherwise.</returns>
    public static string? GetEventId(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("id", out var idElement))
            {
                return idElement.GetString();
            }
        }
        catch
        {
            // Invalid JSON
        }

        return null;
    }

    /// <summary>
    /// Performs a constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="a">First string to compare.</param>
    /// <param name="b">Second string to compare.</param>
    /// <returns>True if strings are equal, false otherwise.</returns>
    private static bool SecureCompare(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}

/// <summary>
/// Represents a Polar webhook event with common properties.
/// </summary>
public record WebhookEvent
{
    /// <summary>
    /// The unique identifier of the event.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The type of the event.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp when the event was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The data associated with the event.
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}

/// <summary>
/// Options for configuring webhook verification.
/// </summary>
public record WebhookVerificationOptions
{
    /// <summary>
    /// The webhook secret.
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    /// The maximum allowed time difference in seconds for timestamp verification.
    /// Default is 300 seconds (5 minutes).
    /// </summary>
    public int MaxTimeDifferenceSeconds { get; init; } = 300;

    /// <summary>
    /// Whether to skip timestamp verification.
    /// Not recommended for production.
    /// </summary>
    public bool SkipTimestampVerification { get; init; } = false;
}