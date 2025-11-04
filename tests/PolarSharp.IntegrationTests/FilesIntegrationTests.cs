using System.Net;
using FluentAssertions;
using PolarSharp.Extensions;
using PolarSharp.Exceptions;
using PolarSharp.Models.Files;
using File = PolarSharp.Models.Files.File;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Files API.
/// </summary>
public class FilesIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public FilesIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListAsync_ShouldReturnFiles()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Files.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Files.ListAsync(page: 1, limit: 5);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().Be(1);
    }

    [Fact]
    public async Task ListAllAsync_ShouldReturnAllFiles()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fileCount = 0;

        // Act
        await foreach (var file in client.Files.ListAllAsync())
        {
            fileCount++;
            file.Should().NotBeNull();
            file.Id.Should().NotBeNullOrEmpty();
            file.Name.Should().NotBeNullOrEmpty();
            file.MimeType.Should().NotBeNullOrEmpty();
        }

        // Assert
        fileCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateFile()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new FileCreateRequest
        {
            Name = $"test-file-{Guid.NewGuid()}.txt",
            Description = "Test file created via integration test",
            MimeType = "text/plain",
            Size = 1024,
            Public = false,
            Metadata = new Dictionary<string, object>
            {
                ["test"] = "integration",
                ["source"] = "FilesIntegrationTests"
            }
        };

        // Act
        var file = await client.Files.CreateAsync(request);

        // Assert
        file.Should().NotBeNull();
        file.Id.Should().NotBeNullOrEmpty();
        file.Name.Should().Be(request.Name);
        file.Description.Should().Be(request.Description);
        file.MimeType.Should().Be(request.MimeType);
        file.Size.Should().Be(request.Size);
        file.Public.Should().Be(request.Public);
        file.Status.Should().Be(FileStatus.Pending);
        file.UploadUrl.Should().NotBeNullOrEmpty();
        file.Metadata.Should().NotBeNull();
        file.Metadata.Should().ContainKey("test");
        file.Metadata["test"].Should().Be("integration");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFile()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdFile = await CreateTestFileAsync(client);

        // Act
        var retrievedFile = await client.Files.GetAsync(createdFile.Id);

        // Assert
        retrievedFile.Should().NotBeNull();
        retrievedFile.Id.Should().Be(createdFile.Id);
        retrievedFile.Name.Should().Be(createdFile.Name);
        retrievedFile.MimeType.Should().Be(createdFile.MimeType);
        retrievedFile.Size.Should().Be(createdFile.Size);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFile()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdFile = await CreateTestFileAsync(client);
        var updateRequest = new FileUpdateRequest
        {
            Name = $"updated-{createdFile.Name}",
            Description = "Updated description",
            Public = true,
            Metadata = new Dictionary<string, object>
            {
                ["updated"] = true,
                ["timestamp"] = DateTime.UtcNow.ToString("O")
            }
        };

        // Act
        var updatedFile = await client.Files.UpdateAsync(createdFile.Id, updateRequest);

        // Assert
        updatedFile.Should().NotBeNull();
        updatedFile.Id.Should().Be(createdFile.Id);
        updatedFile.Name.Should().Be(updateRequest.Name);
        updatedFile.Description.Should().Be(updateRequest.Description);
        updatedFile.Public.Should().Be(updateRequest.Public.Value);
        updatedFile.Metadata.Should().NotBeNull();
        updatedFile.Metadata.Should().ContainKey("updated");
        updatedFile.Metadata["updated"].Should().Be(true);
    }

    [Fact]
    public async Task CompleteUploadAsync_ShouldMarkFileAsUploaded()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdFile = await CreateTestFileAsync(client);
        var completeRequest = new FileUploadCompleteRequest
        {
            Checksum = "test-checksum-12345",
            Metadata = new Dictionary<string, object>
            {
                ["upload_completed"] = true,
                ["completion_time"] = DateTime.UtcNow.ToString("O")
            }
        };

        // Act
        var completedFile = await client.Files.CompleteUploadAsync(createdFile.Id, completeRequest);

        // Assert
        completedFile.Should().NotBeNull();
        completedFile.Id.Should().Be(createdFile.Id);
        completedFile.Status.Should().Be(FileStatus.Uploaded);
        completedFile.Checksum.Should().Be(completeRequest.Checksum);
        completedFile.Metadata.Should().NotBeNull();
        completedFile.Metadata.Should().ContainKey("upload_completed");
        completedFile.Metadata["upload_completed"].Should().Be(true);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteFile()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdFile = await CreateTestFileAsync(client);

        // Act
        var deletedFile = await client.Files.DeleteAsync(createdFile.Id);

        // Assert
        deletedFile.Should().NotBeNull();
        deletedFile.Id.Should().Be(createdFile.Id);
        
        // Verify file is deleted by trying to get it (should fail)
        var exception = await Assert.ThrowsAsync<PolarApiException>(
            () => client.Files.GetAsync(createdFile.Id));
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var builder = client.Files.Query()
            .WithPublic(false)
            .WithMimeType("text/plain");

        // Act
        var response = await client.Files.ListAsync(builder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        
        // Verify filtering (if any files exist)
        foreach (var file in response.Items)
        {
            file.Public.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ListAsync_WithDateFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var builder = client.Files.Query()
            .CreatedAfter(yesterday)
            .CreatedBefore(tomorrow);

        // Act
        var response = await client.Files.ListAsync(builder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        
        // Verify date filtering (if any files exist)
        foreach (var file in response.Items)
        {
            file.CreatedAt.Should().BeOnOrAfter(yesterday);
            file.CreatedAt.Should().BeOnOrBefore(tomorrow);
        }
    }

    [Fact]
    public async Task CreateAsync_WithExpiration_ShouldCreateFileWithExpiration()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var expirationDate = DateTime.UtcNow.AddDays(7);
        var request = new FileCreateRequest
        {
            Name = $"expiring-file-{Guid.NewGuid()}.txt",
            MimeType = "text/plain",
            Size = 512,
            Public = true,
            ExpiresAt = expirationDate
        };

        // Act
        var file = await client.Files.CreateAsync(request);

        // Assert
        file.Should().NotBeNull();
        file.Id.Should().NotBeNullOrEmpty();
        file.Name.Should().Be(request.Name);
        file.ExpiresAt.Should().BeCloseTo(expirationDate, TimeSpan.FromSeconds(1));
        file.Public.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithChecksum_ShouldCreateFileWithChecksum()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var checksum = "sha256:abc123def456789";
        var request = new FileCreateRequest
        {
            Name = $"checksum-file-{Guid.NewGuid()}.txt",
            MimeType = "text/plain",
            Size = 2048,
            Checksum = checksum
        };

        // Act
        var file = await client.Files.CreateAsync(request);

        // Assert
        file.Should().NotBeNull();
        file.Id.Should().NotBeNullOrEmpty();
        file.Checksum.Should().Be(checksum);
    }

    [Fact]
    public async Task ListAsync_WithOrganizationFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var organizationId = Guid.NewGuid().ToString();
        var builder = client.Files.Query()
            .WithOrganizationId(organizationId);

        // Act
        var response = await client.Files.ListAsync(builder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAsync_WithNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var builder = client.Files.Query()
            .WithName("test");

        // Act
        var response = await client.Files.ListAsync(builder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        
        // Verify name filtering (if any files exist)
        foreach (var file in response.Items)
        {
            file.Name.Should().Contain("test");
        }
    }

    [Fact]
    public async Task CreateAsync_WithMinimalData_ShouldCreateFile()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new FileCreateRequest
        {
            Name = $"minimal-file-{Guid.NewGuid()}.txt",
            MimeType = "text/plain",
            Size = 100
        };

        // Act
        var file = await client.Files.CreateAsync(request);

        // Assert
        file.Should().NotBeNull();
        file.Id.Should().NotBeNullOrEmpty();
        file.Name.Should().Be(request.Name);
        file.MimeType.Should().Be(request.MimeType);
        file.Size.Should().Be(request.Size);
        file.Description.Should().BeNull();
        file.Public.Should().BeFalse();
        file.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithPartialData_ShouldUpdateOnlySpecifiedFields()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdFile = await CreateTestFileAsync(client);
        var originalName = createdFile.Name;
        var updateRequest = new FileUpdateRequest
        {
            Description = "Updated description only"
        };

        // Act
        var updatedFile = await client.Files.UpdateAsync(createdFile.Id, updateRequest);

        // Assert
        updatedFile.Should().NotBeNull();
        updatedFile.Id.Should().Be(createdFile.Id);
        updatedFile.Name.Should().Be(originalName); // Should remain unchanged
        updatedFile.Description.Should().Be(updateRequest.Description);
    }

    private async Task<File> CreateTestFileAsync(PolarClient client)
    {
        var request = new FileCreateRequest
        {
            Name = $"test-file-{Guid.NewGuid()}.txt",
            Description = "Test file for integration tests",
            MimeType = "text/plain",
            Size = 1024,
            Public = false,
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["created_at"] = DateTime.UtcNow.ToString("O")
            }
        };

        return await client.Files.CreateAsync(request);
    }
}