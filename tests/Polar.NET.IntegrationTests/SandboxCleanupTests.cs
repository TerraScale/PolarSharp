using Polar.NET;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Polar.NET.IntegrationTests;

/// <summary>
/// Tests for sandbox cleanup functionality.
/// </summary>
public class SandboxCleanupTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SandboxCleanupTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task SandboxCleanup_CleanupAllResources_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var cleanup = new SandboxCleanup(client, _output);

        // Act & Assert
        // This test mainly verifies that the cleanup doesn't throw exceptions
        await cleanup.CleanupAllResourcesAsync();
        
        // If we get here without exceptions, the cleanup worked
        true.Should().BeTrue();
    }
}