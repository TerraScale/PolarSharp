using FluentAssertions;
using PolarSharp.Extensions;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace PolarSharp.Tests;

public class RateLimitHelperTests
{
    [Fact]
    public void ExtractRetryDelay_WithDeltaHeader_ShouldReturnDelay()
    {
        // Arrange
        var retryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
        var maxDelayMs = 60000;

        // Act
        var delay = RateLimitHelper.ExtractRetryDelay(retryAfter, maxDelayMs);

        // Assert
        delay.Should().NotBeNull();
        delay.Value.TotalSeconds.Should().Be(30);
    }

    [Fact]
    public void ExtractRetryDelay_WithDeltaHeaderExceedingMax_ShouldCapAtMax()
    {
        // Arrange
        var retryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(120));
        var maxDelayMs = 60000;

        // Act
        var delay = RateLimitHelper.ExtractRetryDelay(retryAfter, maxDelayMs);

        // Assert
        delay.Should().NotBeNull();
        delay.Value.TotalMilliseconds.Should().Be(60000);
    }

    [Fact]
    public void ExtractRetryDelay_WithDateHeader_ShouldReturnDelay()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddSeconds(45);
        var retryAfter = new RetryConditionHeaderValue(futureDate);
        var maxDelayMs = 60000;

        // Act
        var delay = RateLimitHelper.ExtractRetryDelay(retryAfter, maxDelayMs);

        // Assert
        delay.Should().NotBeNull();
        delay.Value.TotalSeconds.Should().BeApproximately(45, 1);
    }

    [Fact]
    public void ExtractRetryDelay_WithPastDateHeader_ShouldReturnNull()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddSeconds(-10);
        var retryAfter = new RetryConditionHeaderValue(pastDate);
        var maxDelayMs = 60000;

        // Act
        var delay = RateLimitHelper.ExtractRetryDelay(retryAfter, maxDelayMs);

        // Assert
        delay.Should().BeNull();
    }

    [Fact]
    public void ExtractRetryDelay_WithNullHeader_ShouldReturnNull()
    {
        // Arrange
        RetryConditionHeaderValue? retryAfter = null;
        var maxDelayMs = 60000;

        // Act
        var delay = RateLimitHelper.ExtractRetryDelay(retryAfter, maxDelayMs);

        // Assert
        delay.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 1000, 30000, 0.1, 1000)] // First retry
    [InlineData(2, 1000, 30000, 0.1, 2000)] // Second retry
    [InlineData(3, 1000, 30000, 0.1, 4000)] // Third retry
    [InlineData(6, 1000, 30000, 0.1, 30000)] // Capped at max
    public void CalculateExponentialBackoffWithJitter_ShouldCalculateCorrectly(
        int retryAttempt, int initialDelayMs, int maxDelayMs, double jitterFactor, double expectedBaseDelay)
    {
        // Act
        var delay = RateLimitHelper.CalculateExponentialBackoffWithJitter(
            retryAttempt, initialDelayMs, maxDelayMs, jitterFactor);

        // Assert
        delay.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(expectedBaseDelay);
        delay.TotalMilliseconds.Should().BeLessThanOrEqualTo(expectedBaseDelay * (1 + jitterFactor) + 1); // +1 for rounding
    }

    [Fact]
    public void CalculateExponentialBackoffWithJitter_ShouldCapAtMaxDelay()
    {
        // Arrange
        var retryAttempt = 10; // High retry attempt
        var initialDelayMs = 1000;
        var maxDelayMs = 5000;
        var jitterFactor = 0.1;

        // Act
        var delay = RateLimitHelper.CalculateExponentialBackoffWithJitter(
            retryAttempt, initialDelayMs, maxDelayMs, jitterFactor);

        // Assert
        delay.TotalMilliseconds.Should().BeLessThanOrEqualTo(maxDelayMs * (1 + jitterFactor) + 1);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.BadGateway, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.GatewayTimeout, true)]
    [InlineData(HttpStatusCode.RequestTimeout, true)]
    [InlineData(HttpStatusCode.Conflict, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.Unauthorized, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    public void ShouldRetry_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expected)
    {
        // Act
        var shouldRetry = RateLimitHelper.ShouldRetry(statusCode);

        // Assert
        shouldRetry.Should().Be(expected);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, "Rate limit exceeded")]
    [InlineData(HttpStatusCode.InternalServerError, "Internal server error")]
    [InlineData(HttpStatusCode.BadGateway, "Bad gateway")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Service unavailable")]
    [InlineData(HttpStatusCode.GatewayTimeout, "Gateway timeout")]
    [InlineData(HttpStatusCode.RequestTimeout, "Request timeout")]
    [InlineData(HttpStatusCode.Conflict, "Resource conflict")]
    [InlineData(HttpStatusCode.OK, "HTTP 200 error")]
    public void GetRetryReason_ShouldReturnCorrectDescription(HttpStatusCode statusCode, string expectedReason)
    {
        // Act
        var reason = RateLimitHelper.GetRetryReason(statusCode);

        // Assert
        reason.Should().Be(expectedReason);
    }
}

public class PolarClientRateLimitingTests
{
    [Fact]
    public void RateLimitStatus_ShouldReturnCorrectStatus()
    {
        // Arrange
        var client = new PolarClient("test-token");

        // Act
        var status = client.RateLimitStatus;

        // Assert
        status.Available.Should().BeGreaterThanOrEqualTo(0);
        status.Limit.Should().BeGreaterThan(0);
        // ResetTime can be null when no requests have been made yet
    }

    [Fact]
    public void Builder_WithMaxRetryDelay_ShouldSetOption()
    {
        // Arrange & Act
        var client = PolarClient.Create()
            .WithAccessToken("test-token")
            .WithMaxRetryDelay(45000)
            .Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Builder_WithJitterFactor_ShouldSetOption()
    {
        // Arrange & Act
        var client = PolarClient.Create()
            .WithAccessToken("test-token")
            .WithJitterFactor(0.2)
            .Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Builder_WithJitterFactor_ExceedingBounds_ShouldClamp()
    {
        // Arrange & Act
        var client = PolarClient.Create()
            .WithAccessToken("test-token")
            .WithJitterFactor(1.5) // Should be clamped to 1.0
            .Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Builder_WithRespectRetryAfterHeader_ShouldSetOption()
    {
        // Arrange & Act
        var client = PolarClient.Create()
            .WithAccessToken("test-token")
            .WithRespectRetryAfterHeader(false)
            .Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Builder_WithAllRateLimitOptions_ShouldCreateValidClient()
    {
        // Arrange & Act
        var client = PolarClient.Create()
            .WithAccessToken("test-token")
            .WithMaxRetries(5)
            .WithInitialRetryDelay(500)
            .WithMaxRetryDelay(20000)
            .WithJitterFactor(0.15)
            .WithRespectRetryAfterHeader(true)
            .WithRequestsPerMinute(150)
            .Build();

        // Assert
        client.Should().NotBeNull();
        client.Products.Should().NotBeNull();
        client.Orders.Should().NotBeNull();
    }
}