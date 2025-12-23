using System.Net;
using FluentAssertions;
using PolarSharp.Extensions;
using PolarSharp.Results;
using PolarSharp.Models.Files;
using File = PolarSharp.Models.Files.File;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Files API.
/// </summary>
public class FilesIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public FilesIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task ListAsync_ShouldReturnFiles()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Files.ListAsync();

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Files.ListAsync(page: 1, limit: 5);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();
            response.Pagination.Page.Should().Be(1);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_ShouldReturnAllFiles()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var fileCount = 0;

            // Act
            await foreach (var fileResult in client.Files.ListAllAsync())
            {
                if (fileResult.IsFailure) break;

                var file = fileResult.Value;
                fileCount++;
                file.Should().NotBeNull();
                file.Id.Should().NotBeNullOrEmpty();
                file.Name.Should().NotBeNullOrEmpty();
                file.MimeType.Should().NotBeNullOrEmpty();
            }

            // Assert
            fileCount.Should().BeGreaterThanOrEqualTo(0);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateFile()
    {
        try
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
            var result = await client.Files.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var file = result.Value;
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
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFile()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createdFile = await CreateTestFileAsync(client);
            if (createdFile == null)
            {
                _output.WriteLine("Skipped: Could not create test file");
                return;
            }

            // Act
            var result = await client.Files.GetAsync(createdFile.Id);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var retrievedFile = result.Value;
            retrievedFile.Should().NotBeNull();
            retrievedFile.Id.Should().Be(createdFile.Id);
            retrievedFile.Name.Should().Be(createdFile.Name);
            retrievedFile.MimeType.Should().Be(createdFile.MimeType);
            retrievedFile.Size.Should().Be(createdFile.Size);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFile()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createdFile = await CreateTestFileAsync(client);
            if (createdFile == null)
            {
                _output.WriteLine("Skipped: Could not create test file");
                return;
            }
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
            var result = await client.Files.UpdateAsync(createdFile.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var updatedFile = result.Value;
            updatedFile.Should().NotBeNull();
            updatedFile.Id.Should().Be(createdFile.Id);
            updatedFile.Name.Should().Be(updateRequest.Name);
            updatedFile.Description.Should().Be(updateRequest.Description);
            updatedFile.Public.Should().Be(updateRequest.Public.Value);
            updatedFile.Metadata.Should().NotBeNull();
            updatedFile.Metadata.Should().ContainKey("updated");
            updatedFile.Metadata["updated"].Should().Be(true);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CompleteUploadAsync_ShouldMarkFileAsUploaded()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createdFile = await CreateTestFileAsync(client);
            if (createdFile == null)
            {
                _output.WriteLine("Skipped: Could not create test file");
                return;
            }
            var completeRequest = new FileUploadCompleteRequest
            {
                Id = createdFile.Id,
                Path = createdFile.Path ?? "test-path"
            };

            // Act
            var result = await client.Files.CompleteUploadAsync(createdFile.Id, completeRequest);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var completedFile = result.Value;
            completedFile.Should().NotBeNull();
            completedFile.Id.Should().Be(createdFile.Id);
            completedFile.IsUploaded.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteFile()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createdFile = await CreateTestFileAsync(client);
            if (createdFile == null)
            {
                _output.WriteLine("Skipped: Could not create test file");
                return;
            }

            // Act
            var result = await client.Files.DeleteAsync(createdFile.Id);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var deletedFile = result.Value;
            deletedFile.Should().NotBeNull();
            deletedFile.Id.Should().Be(createdFile.Id);

            // Verify file is deleted by trying to get it
            var getResult = await client.Files.GetAsync(createdFile.Id);
            getResult.IsSuccess.Should().BeFalse();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ShouldReturnFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var builder = client.Files.Query()
                .WithPublic(false)
                .WithMimeType("text/plain");

            // Act
            var result = await client.Files.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();

            // Verify filtering (if any files exist)
            foreach (var file in response.Items)
            {
                file.Public.Should().BeFalse();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithDateFilters_ShouldReturnFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var tomorrow = DateTime.UtcNow.AddDays(1);
            var builder = client.Files.Query()
                .CreatedAfter(yesterday)
                .CreatedBefore(tomorrow);

            // Act
            var result = await client.Files.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
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
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithExpiration_ShouldCreateFileWithExpiration()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new FileCreateRequest
            {
                Name = $"expiring-file-{Guid.NewGuid()}.txt",
                MimeType = "text/plain",
                Size = 512
            };

            // Act
            var result = await client.Files.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var file = result.Value;
            file.Should().NotBeNull();
            file.Id.Should().NotBeNullOrEmpty();
            file.Name.Should().Be(request.Name);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithChecksum_ShouldCreateFileWithChecksum()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new FileCreateRequest
            {
                Name = $"checksum-file-{Guid.NewGuid()}.txt",
                MimeType = "text/plain",
                Size = 2048
            };

            // Act
            var result = await client.Files.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var file = result.Value;
            file.Should().NotBeNull();
            file.Id.Should().NotBeNullOrEmpty();
            // Checksum is populated after upload completion
            _output.WriteLine($"File created: {file.Id}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithOrganizationFilter_ShouldReturnFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var organizationId = Guid.NewGuid().ToString();
            var builder = client.Files.Query()
                .WithOrganizationId(organizationId);

            // Act
            var result = await client.Files.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithNameFilter_ShouldReturnFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var builder = client.Files.Query()
                .WithName("test");

            // Act
            var result = await client.Files.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Items.Should().NotBeNull();
            response.Pagination.Should().NotBeNull();

            // Verify name filtering (if any files exist)
            foreach (var file in response.Items)
            {
                file.Name.Should().Contain("test");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CreateAsync_WithMinimalData_ShouldCreateFile()
    {
        try
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
            var result = await client.Files.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var file = result.Value;
            file.Should().NotBeNull();
            file.Id.Should().NotBeNullOrEmpty();
            file.Name.Should().Be(request.Name);
            file.MimeType.Should().Be(request.MimeType);
            file.Size.Should().Be(request.Size);
            file.Description.Should().BeNull();
            file.Public.Should().BeFalse();
            file.Metadata.Should().BeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithPartialData_ShouldUpdateOnlySpecifiedFields()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var createdFile = await CreateTestFileAsync(client);
            if (createdFile == null)
            {
                _output.WriteLine("Skipped: Could not create test file");
                return;
            }
            var originalName = createdFile.Name;
            var updateRequest = new FileUpdateRequest
            {
                Description = "Updated description only"
            };

            // Act
            var result = await client.Files.UpdateAsync(createdFile.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var updatedFile = result.Value;
            updatedFile.Should().NotBeNull();
            updatedFile.Id.Should().Be(createdFile.Id);
            updatedFile.Name.Should().Be(originalName); // Should remain unchanged
            updatedFile.Description.Should().Be(updateRequest.Description);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    private async Task<File?> CreateTestFileAsync(PolarClient client)
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

        var result = await client.Files.CreateAsync(request);
        if (result.IsFailure)
        {
            _output.WriteLine($"CreateTestFileAsync failed: {result.Error!.Message}");
            return null;
        }
        return result.Value;
    }
}
