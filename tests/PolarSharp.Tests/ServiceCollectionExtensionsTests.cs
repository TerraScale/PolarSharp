using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using Xunit;

namespace PolarSharp.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPolarClient_WithAccessToken_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var accessToken = "test-access-token";

        // Act
        services.AddPolarClient(accessToken);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var polarClient = serviceProvider.GetService<IPolarClient>();
        var polarClientConcrete = serviceProvider.GetService<PolarClient>();
        var options = serviceProvider.GetService<PolarClientOptions>();

        Assert.NotNull(polarClient);
        Assert.NotNull(polarClientConcrete);
        Assert.NotNull(options);
        Assert.Equal(accessToken, options.AccessToken);
        Assert.Equal(PolarEnvironment.Production, options.Environment);
    }

    [Fact]
    public void AddPolarClient_WithConfiguration_ShouldUseProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var accessToken = "test-access-token";

        // Act
        services.AddPolarClient(options =>
        {
            options.AccessToken = accessToken;
            options.Environment = PolarEnvironment.Sandbox;
            options.TimeoutSeconds = 60;
            options.MaxRetryAttempts = 5;
            options.RequestsPerMinute = 250;
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<PolarClientOptions>();

        Assert.NotNull(options);
        Assert.Equal(accessToken, options.AccessToken);
        Assert.Equal(PolarEnvironment.Sandbox, options.Environment);
        Assert.Equal(60, options.TimeoutSeconds);
        Assert.Equal(5, options.MaxRetryAttempts);
        Assert.Equal(250, options.RequestsPerMinute);
    }

    [Fact]
    public void AddPolarClientSandbox_ShouldSetSandboxEnvironment()
    {
        // Arrange
        var services = new ServiceCollection();
        var accessToken = "test-access-token";

        // Act
        services.AddPolarClientSandbox(accessToken);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<PolarClientOptions>();

        Assert.NotNull(options);
        Assert.Equal(accessToken, options.AccessToken);
        Assert.Equal(PolarEnvironment.Sandbox, options.Environment);
    }

    [Fact]
    public void AddPolarClientProduction_ShouldSetProductionEnvironment()
    {
        // Arrange
        var services = new ServiceCollection();
        var accessToken = "test-access-token";

        // Act
        services.AddPolarClientProduction(accessToken);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<PolarClientOptions>();

        Assert.NotNull(options);
        Assert.Equal(accessToken, options.AccessToken);
        Assert.Equal(PolarEnvironment.Production, options.Environment);
    }

    [Fact]
    public void AddPolarClient_WithPreConfiguredOptions_ShouldUseOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new PolarClientOptions
        {
            AccessToken = "test-access-token",
            Environment = PolarEnvironment.Sandbox,
            TimeoutSeconds = 45,
            MaxRetryAttempts = 2
        };

        // Act
        services.AddPolarClient(options);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var registeredOptions = serviceProvider.GetService<PolarClientOptions>();

        Assert.NotNull(registeredOptions);
        Assert.Equal(options.AccessToken, registeredOptions.AccessToken);
        Assert.Equal(options.Environment, registeredOptions.Environment);
        Assert.Equal(options.TimeoutSeconds, registeredOptions.TimeoutSeconds);
        Assert.Equal(options.MaxRetryAttempts, registeredOptions.MaxRetryAttempts);
    }

    [Fact]
    public void AddPolarClient_WithNullAccessToken_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddPolarClient((string)null!));
    }

    [Fact]
    public void AddPolarClient_WithEmptyAccessToken_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddPolarClient((string)""));
    }

    [Fact]
    public void AddPolarClient_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddPolarClient((PolarClientOptions)null!));
    }

    [Fact]
    public void AddPolarClient_WithConfigurationMissingAccessToken_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddPolarClient(options =>
        {
            options.Environment = PolarEnvironment.Sandbox;
            // Missing AccessToken
        }));
    }
}