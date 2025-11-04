using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Polar.NET.Extensions;

/// <summary>
/// Helper class for verifying Polar webhook signatures.
/// </summary>
public static class WebhookSignature
{
    /// <summary>
    /// Verifies a Polar webhook signature.
    /// </summary>
    /// <param name="payload">The raw webhook payload (JSON string).</param>
    /// <param name="signature">The signature from the X-Polar-Signature header.</param>
    /// <param name="secret">The webhook secret from your Polar dashboard.</param>
    /// <returns>true if the signature is valid; otherwise, false.</returns>
    public static bool Verify(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        try
        {
            // Extract the timestamp and signature from the header
            // Format: "t=timestamp,v1=signature"
            var parts = signature.Split(',');
            var timestampPart = parts.FirstOrDefault(p => p.StartsWith("t="));
            var signaturePart = parts.FirstOrDefault(p => p.StartsWith("v1="));

            if (timestampPart == null || signaturePart == null)
            {
                return false;
            }

            var timestamp = timestampPart[2..]; // Remove "t=" prefix
            var expectedSignature = signaturePart[3..]; // Remove "v1=" prefix

            // Check if timestamp is within 5 minutes (300 seconds)
            if (!long.TryParse(timestamp, out var timestampSeconds))
            {
                return false;
            }

            var timestampTime = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds);
            var now = DateTimeOffset.UtcNow;
            var timeDifference = Math.Abs((now - timestampTime).TotalSeconds);

            if (timeDifference > 300) // 5 minutes
            {
                return false;
            }

            // Create the signed payload
            var signedPayload = $"{timestamp}.{payload}";
            var expectedBytes = ComputeHmacSha256(signedPayload, secret);
            var expectedSignatureHex = Convert.ToHexString(expectedBytes).ToLowerInvariant();

            // Constant-time comparison to prevent timing attacks
            return ConstantTimeEquals(expectedSignature, expectedSignatureHex);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifies a Polar webhook signature using the raw request body.
    /// </summary>
    /// <param name="requestBody">The raw request body bytes.</param>
    /// <param name="signature">The signature from the X-Polar-Signature header.</param>
    /// <param name="secret">The webhook secret from your Polar dashboard.</param>
    /// <returns>true if the signature is valid; otherwise, false.</returns>
    public static bool Verify(byte[] requestBody, string signature, string secret)
    {
        if (requestBody == null || requestBody.Length == 0)
        {
            return false;
        }

        var payload = Encoding.UTF8.GetString(requestBody);
        return Verify(payload, signature, secret);
    }

    /// <summary>
    /// Computes HMAC-SHA256 hash of the input data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The secret key.</param>
    /// <returns>The HMAC-SHA256 hash bytes.</returns>
    private static byte[] ComputeHmacSha256(string data, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        return hmac.ComputeHash(dataBytes);
    }

    /// <summary>
    /// Performs a constant-time comparison between two strings to prevent timing attacks.
    /// </summary>
    /// <param name="a">First string.</param>
    /// <param name="b">Second string.</param>
    /// <returns>true if strings are equal; otherwise, false.</returns>
    private static bool ConstantTimeEquals(string a, string b)
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
/// Represents a parsed Polar webhook event.
/// </summary>
public class PolarWebhookEvent
{
    /// <summary>
    /// The event type (e.g., "order.created", "customer.updated").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The event data payload.
    /// </summary>
    public JsonElement Data { get; set; }

    /// <summary>
    /// The webhook ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the event was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Parses a webhook event from JSON payload.
    /// </summary>
    /// <param name="jsonPayload">The JSON payload from the webhook.</param>
    /// <returns>A parsed webhook event.</returns>
    public static PolarWebhookEvent Parse(string jsonPayload)
    {
        using var document = JsonDocument.Parse(jsonPayload);
        var root = document.RootElement;

        return new PolarWebhookEvent
        {
            Type = root.GetProperty("type").GetString() ?? string.Empty,
            Data = root.GetProperty("data"),
            Id = root.GetProperty("id").GetString() ?? string.Empty,
            CreatedAt = root.GetProperty("created_at").GetDateTime()
        };
    }

    /// <summary>
    /// Gets the event data as a strongly typed object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized event data.</returns>
    public T? GetData<T>() where T : class
    {
        return Data.Deserialize<T>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        });
    }
}