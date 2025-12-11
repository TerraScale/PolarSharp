using Microsoft.Extensions.Configuration;
using PolarSharp;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Test fixture for integration tests.
/// </summary>
public class IntegrationTestFixture
{
    private readonly IConfiguration _configuration;

    public IntegrationTestFixture()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
    }

    /// <summary>
    /// Creates a PolarClient configured for sandbox testing.
    /// </summary>
    /// <returns>A configured PolarClient instance.</returns>
    public PolarClient CreateClient()
    {
        var accessToken = _configuration["Polar:AccessToken"] ?? Environment.GetEnvironmentVariable("POLAR_ACCESS_TOKEN");
        
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException(
                "Polar access token not configured. Set POLAR_ACCESS_TOKEN environment variable or add Polar:AccessToken to appsettings.json");
        }

        return PolarClient.Create()
            .WithToken(accessToken)
            .WithEnvironment(Models.Common.PolarEnvironment.Sandbox)
            .WithTimeout(30)
            .WithMaxRetries(2) // Further reduce retries for tests
            .WithInitialRetryDelay(2000) // Longer initial delay
            .WithRequestsPerMinute(15) // Even more conservative rate limiting for tests
            .Build();
    }

    /// <summary>
    /// Creates a PolarClient with a specific token for testing.
    /// </summary>
    /// <param name="accessToken">The access token to use.</param>
    /// <returns>A configured PolarClient instance.</returns>
    public PolarClient CreateClient(string accessToken)
    {
        return PolarClient.Create()
            .WithToken(accessToken)
            .WithEnvironment(Models.Common.PolarEnvironment.Sandbox)
            .WithTimeout(30)
            .WithMaxRetries(5) // Increase retries for tests
            .WithInitialRetryDelay(2000) // Longer initial delay
            .WithRequestsPerMinute(120) // More conservative rate limiting for tests
            .Build();
    }
}