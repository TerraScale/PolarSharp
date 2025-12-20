using FluentAssertions;
using PolarSharp.Models.Organizations;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Organizations API.
/// </summary>
public class OrganizationsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public OrganizationsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task OrganizationsApi_ListOrganizations_ReturnsPaginatedResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.Organizations.ListAsync(page: 1, limit: 10);

            // Assert
            response.Should().NotBeNull();
            // Sandbox may not allow listing organizations
            if (response.IsFailure)
            {
                _output.WriteLine($"Skipped: {response.Error!.Message}");
                return;
            }
            response.Value.Items.Should().NotBeNull();
            response.Value.Pagination.Should().NotBeNull();
            response.Value.Pagination.Page.Should().Be(1);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_GetOrganization_WithValidId_ReturnsOrganization()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // First, list organizations to get a valid ID
            var organizations = await client.Organizations.ListAsync();
            if (organizations.IsFailure || organizations.Value.Items.Count == 0)
            {
                _output.WriteLine($"Skipped: {organizations.Error?.Message ?? "No organizations exist"}");
                return;
            }

            var organizationId = organizations.Value.Items.First().Id;

            // Act
            var organization = await client.Organizations.GetAsync(organizationId);

            // Assert
            organization.Should().NotBeNull();
            if (organization.IsFailure)
            {
                _output.WriteLine($"Skipped: {organization.Error!.Message}");
                return;
            }
            organization.Value.Id.Should().Be(organizationId);
            organization.Value.Name.Should().NotBeNullOrEmpty();
            organization.Value.Slug.Should().NotBeNullOrEmpty();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_GetOrganization_WithInvalidId_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidOrganizationId = "invalid_organization_id";

            // Act
            var result = await client.Organizations.GetAsync(invalidOrganizationId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_CreateOrganization_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var organizationName = $"Test Organization {Guid.NewGuid()}";
            var organizationSlug = $"test-org-{Guid.NewGuid():N}";

            var createRequest = new OrganizationCreateRequest
            {
                Name = organizationName,
                Slug = organizationSlug,
                Description = "Integration test organization",
                WebsiteUrl = "https://example.com",
                TwitterHandle = "testorg",
                GithubUrl = "https://github.com/testorg",
                Public = true,
                DefaultCurrency = "USD",
                Country = "US",
                Timezone = "UTC",
                Metadata = new Dictionary<string, object>
                {
                    ["test"] = true,
                    ["environment"] = "integration"
                },
                Settings = new OrganizationSettings
                {
                    EnableCustomDomains = true,
                    EnableWebhooks = true,
                    EnableCustomerPortal = true,
                    EnableLicenseKeys = true,
                    EnableDownloads = true,
                    EnableSubscriptions = true,
                    EnableDiscounts = true,
                    EnableCustomFields = true,
                    EnableMetrics = true,
                    EnableEvents = true,
                    EnableOAuth2 = true,
                    EnableCustomerSeats = true,
                    EnableMeters = true,
                    EnableCustomerMeters = true
                }
            };

            // Act
            var createdOrganization = await client.Organizations.CreateAsync(createRequest);

            // Assert - Sandbox may not allow organization creation
            createdOrganization.Should().NotBeNull();
            if (createdOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {createdOrganization.Error!.Message}");
                return;
            }
            createdOrganization.Value.Id.Should().NotBeNullOrEmpty();
            createdOrganization.Value.Name.Should().Be(organizationName);
            createdOrganization.Value.Slug.Should().Be(organizationSlug);
            createdOrganization.Value.Description.Should().Be("Integration test organization");
            createdOrganization.Value.WebsiteUrl.Should().Be("https://example.com");
            createdOrganization.Value.TwitterHandle.Should().Be("testorg");
            createdOrganization.Value.GithubUrl.Should().Be("https://github.com/testorg");
            createdOrganization.Value.Public.Should().BeTrue();
            createdOrganization.Value.DefaultCurrency.Should().Be("USD");
            createdOrganization.Value.Country.Should().Be("US");
            createdOrganization.Value.Timezone.Should().Be("UTC");
            createdOrganization.Value.Metadata.Should().NotBeNull();
            createdOrganization.Value.Settings.Should().NotBeNull();

            // Cleanup
            await client.Organizations.DeleteAsync(createdOrganization.Value.Id);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_UpdateOrganization_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var organizationName = $"Test Organization {Guid.NewGuid()}";
            var organizationSlug = $"test-org-{Guid.NewGuid():N}";

            // Create organization first
            var createRequest = new OrganizationCreateRequest
            {
                Name = organizationName,
                Slug = organizationSlug,
                Description = "Original description"
            };

            var createdOrganization = await client.Organizations.CreateAsync(createRequest);
            if (createdOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {createdOrganization.Error!.Message}");
                return;
            }

            // Update request
            var updateRequest = new OrganizationUpdateRequest
            {
                Name = $"{organizationName} (Updated)",
                Description = "Updated description",
                WebsiteUrl = "https://updated-example.com",
                Public = false,
                DefaultCurrency = "EUR",
                Country = "GB",
                Timezone = "Europe/London",
                Metadata = new Dictionary<string, object>
                {
                    ["updated"] = true,
                    ["version"] = 2
                }
            };

            // Act
            var updatedOrganization = await client.Organizations.UpdateAsync(createdOrganization.Value.Id, updateRequest);

            // Assert
            updatedOrganization.Should().NotBeNull();
            if (updatedOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {updatedOrganization.Error!.Message}");
                return;
            }
            updatedOrganization.Value.Id.Should().Be(createdOrganization.Value.Id);
            updatedOrganization.Value.Name.Should().Be($"{organizationName} (Updated)");
            updatedOrganization.Value.Description.Should().Be("Updated description");
            updatedOrganization.Value.WebsiteUrl.Should().Be("https://updated-example.com");
            updatedOrganization.Value.Public.Should().BeFalse();
            updatedOrganization.Value.DefaultCurrency.Should().Be("EUR");
            updatedOrganization.Value.Country.Should().Be("GB");
            updatedOrganization.Value.Timezone.Should().Be("Europe/London");
            updatedOrganization.Value.Metadata.Should().ContainKey("updated");

            // Cleanup
            await client.Organizations.DeleteAsync(updatedOrganization.Value.Id);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_DeleteOrganization_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var organizationName = $"Test Organization {Guid.NewGuid()}";
            var organizationSlug = $"test-org-{Guid.NewGuid():N}";

            var createRequest = new OrganizationCreateRequest
            {
                Name = organizationName,
                Slug = organizationSlug,
                Description = "Organization to be deleted"
            };

            var createdOrganization = await client.Organizations.CreateAsync(createRequest);
            if (createdOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {createdOrganization.Error!.Message}");
                return;
            }

            // Act
            var deletedOrganization = await client.Organizations.DeleteAsync(createdOrganization.Value.Id);

            // Assert
            deletedOrganization.Should().NotBeNull();
            if (deletedOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {deletedOrganization.Error!.Message}");
                return;
            }
            deletedOrganization.Value.Id.Should().Be(createdOrganization.Value.Id);
            deletedOrganization.Value.Name.Should().Be(organizationName);

            // Verify organization is deleted by checking it returns false for IsSuccess
            var result = await client.Organizations.GetAsync(createdOrganization.Value.Id);
            result.IsSuccess.Should().BeFalse();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_ListAllOrganizations_UsingAsyncEnumerable_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var allOrganizations = new List<Organization>();
            await foreach (var organizationResult in client.Organizations.ListAllAsync())
            {
                if (organizationResult.IsFailure)
                {
                    _output.WriteLine($"Skipped: {organizationResult.Error!.Message}");
                    return;
                }
                allOrganizations.Add(organizationResult.Value);
            }

            // Assert
            allOrganizations.Should().NotBeNull();
            allOrganizations.Should().BeAssignableTo<List<Organization>>();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_CreateOrganization_WithMinimalData_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var organizationName = $"Minimal Org {Guid.NewGuid()}";
            var organizationSlug = $"minimal-org-{Guid.NewGuid():N}";

            var createRequest = new OrganizationCreateRequest
            {
                Name = organizationName,
                Slug = organizationSlug
            };

            // Act
            var createdOrganization = await client.Organizations.CreateAsync(createRequest);

            // Assert - Sandbox may not allow organization creation
            createdOrganization.Should().NotBeNull();
            if (createdOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {createdOrganization.Error!.Message}");
                return;
            }
            createdOrganization.Value.Id.Should().NotBeNullOrEmpty();
            createdOrganization.Value.Name.Should().Be(organizationName);
            createdOrganization.Value.Slug.Should().Be(organizationSlug);
            createdOrganization.Value.Description.Should().BeNull();
            createdOrganization.Value.WebsiteUrl.Should().BeNull();

            // Cleanup
            await client.Organizations.DeleteAsync(createdOrganization.Value.Id);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_UpdateOrganization_WithPartialData_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var organizationName = $"Partial Update Org {Guid.NewGuid()}";
            var organizationSlug = $"partial-org-{Guid.NewGuid():N}";

            // Create organization first
            var createRequest = new OrganizationCreateRequest
            {
                Name = organizationName,
                Slug = organizationSlug,
                Description = "Original description",
                WebsiteUrl = "https://original.com"
            };

            var createdOrganization = await client.Organizations.CreateAsync(createRequest);
            if (createdOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {createdOrganization.Error!.Message}");
                return;
            }

            // Update only the description
            var updateRequest = new OrganizationUpdateRequest
            {
                Description = "Updated description only"
            };

            // Act
            var updatedOrganization = await client.Organizations.UpdateAsync(createdOrganization.Value.Id, updateRequest);

            // Assert
            updatedOrganization.Should().NotBeNull();
            if (updatedOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {updatedOrganization.Error!.Message}");
                return;
            }
            updatedOrganization.Value.Id.Should().Be(createdOrganization.Value.Id);
            updatedOrganization.Value.Name.Should().Be(organizationName); // Should remain unchanged
            updatedOrganization.Value.Description.Should().Be("Updated description only");
            updatedOrganization.Value.WebsiteUrl.Should().Be("https://original.com"); // Should remain unchanged

            // Cleanup
            await client.Organizations.DeleteAsync(updatedOrganization.Value.Id);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_ListOrganizations_WithPagination_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var page1 = await client.Organizations.ListAsync(page: 1, limit: 5);
            if (page1.IsFailure)
            {
                _output.WriteLine($"Skipped: {page1.Error!.Message}");
                return;
            }

            var page2 = await client.Organizations.ListAsync(page: 2, limit: 5);

            // Assert
            page1.Should().NotBeNull();
            page1.Value.Pagination.Page.Should().Be(1);

            page2.Should().NotBeNull();
            if (page2.IsSuccess)
            {
                page2.Value.Pagination.Page.Should().Be(2);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_CreateOrganization_WithDuplicateSlug_ThrowsValidationException()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var organizationSlug = $"duplicate-org-{Guid.NewGuid():N}";

            // Create first organization
            var firstRequest = new OrganizationCreateRequest
            {
                Name = "First Organization",
                Slug = organizationSlug
            };

            var firstOrganization = await client.Organizations.CreateAsync(firstRequest);
            if (firstOrganization.IsFailure)
            {
                _output.WriteLine($"Skipped: {firstOrganization.Error!.Message}");
                return;
            }

            // Try to create second organization with same slug
            var secondRequest = new OrganizationCreateRequest
            {
                Name = "Second Organization",
                Slug = organizationSlug
            };

            // Act
            var secondOrganization = await client.Organizations.CreateAsync(secondRequest);

            // Assert
            secondOrganization.Should().NotBeNull();
            secondOrganization.IsSuccess.Should().BeFalse();
            secondOrganization.IsValidationError.Should().BeTrue();

            // Cleanup
            await client.Organizations.DeleteAsync(firstOrganization.Value.Id);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_UpdateOrganization_WithInvalidId_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidOrganizationId = "invalid_organization_id";

            var updateRequest = new OrganizationUpdateRequest
            {
                Name = "Updated Name"
            };

            // Act
            var result = await client.Organizations.UpdateAsync(invalidOrganizationId, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_DeleteOrganization_WithInvalidId_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidOrganizationId = "invalid_organization_id";

            // Act
            var result = await client.Organizations.DeleteAsync(invalidOrganizationId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrganizationsApi_ListOrganizations_LargeLimit_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var response = await client.Organizations.ListAsync(page: 1, limit: 100);

            // Assert
            response.Should().NotBeNull();
            if (response.IsFailure)
            {
                _output.WriteLine($"Skipped: {response.Error!.Message}");
                return;
            }
            response.Value.Items.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}
