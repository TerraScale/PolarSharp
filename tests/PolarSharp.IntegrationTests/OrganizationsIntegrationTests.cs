using FluentAssertions;
using PolarSharp.Models.Organizations;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Organizations API.
/// </summary>
public class OrganizationsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public OrganizationsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OrganizationsApi_ListOrganizations_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Organizations.ListAsync(page: 1, limit: 10);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().Be(1);
    }

    [Fact]
    public async Task OrganizationsApi_GetOrganization_WithValidId_ReturnsOrganization()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, list organizations to get a valid ID
        var organizations = await client.Organizations.ListAsync();
        if (organizations.Items.Count == 0)
        {
            return; // Skip if no organizations exist
        }

        var organizationId = organizations.Items.First().Id;

        // Act
        var organization = await client.Organizations.GetAsync(organizationId);

        // Assert
        organization.Should().NotBeNull();
        organization.Id.Should().Be(organizationId);
        organization.Name.Should().NotBeNullOrEmpty();
        organization.Slug.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task OrganizationsApi_GetOrganization_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidOrganizationId = "invalid_organization_id";

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Organizations.GetAsync(invalidOrganizationId));
    }

    [Fact]
    public async Task OrganizationsApi_CreateOrganization_WorksCorrectly()
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

        // Assert
        createdOrganization.Should().NotBeNull();
        createdOrganization.Id.Should().NotBeNullOrEmpty();
        createdOrganization.Name.Should().Be(organizationName);
        createdOrganization.Slug.Should().Be(organizationSlug);
        createdOrganization.Description.Should().Be("Integration test organization");
        createdOrganization.WebsiteUrl.Should().Be("https://example.com");
        createdOrganization.TwitterHandle.Should().Be("testorg");
        createdOrganization.GithubUrl.Should().Be("https://github.com/testorg");
        createdOrganization.Public.Should().BeTrue();
        createdOrganization.DefaultCurrency.Should().Be("USD");
        createdOrganization.Country.Should().Be("US");
        createdOrganization.Timezone.Should().Be("UTC");
        createdOrganization.Metadata.Should().NotBeNull();
        createdOrganization.Settings.Should().NotBeNull();

        // Cleanup
        try
        {
            await client.Organizations.DeleteAsync(createdOrganization.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OrganizationsApi_UpdateOrganization_WorksCorrectly()
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
        var updatedOrganization = await client.Organizations.UpdateAsync(createdOrganization.Id, updateRequest);

        // Assert
        updatedOrganization.Should().NotBeNull();
        updatedOrganization.Id.Should().Be(createdOrganization.Id);
        updatedOrganization.Name.Should().Be($"{organizationName} (Updated)");
        updatedOrganization.Description.Should().Be("Updated description");
        updatedOrganization.WebsiteUrl.Should().Be("https://updated-example.com");
        updatedOrganization.Public.Should().BeFalse();
        updatedOrganization.DefaultCurrency.Should().Be("EUR");
        updatedOrganization.Country.Should().Be("GB");
        updatedOrganization.Timezone.Should().Be("Europe/London");
        updatedOrganization.Metadata.Should().ContainKey("updated");

        // Cleanup
        try
        {
            await client.Organizations.DeleteAsync(updatedOrganization.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OrganizationsApi_DeleteOrganization_WorksCorrectly()
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

        // Act
        var deletedOrganization = await client.Organizations.DeleteAsync(createdOrganization.Id);

        // Assert
        deletedOrganization.Should().NotBeNull();
        deletedOrganization.Id.Should().Be(createdOrganization.Id);
        deletedOrganization.Name.Should().Be(organizationName);

        // Verify organization is deleted
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Organizations.GetAsync(createdOrganization.Id));
    }

    [Fact]
    public async Task OrganizationsApi_ListAllOrganizations_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allOrganizations = new List<Organization>();
        await foreach (var organization in client.Organizations.ListAllAsync())
        {
            allOrganizations.Add(organization);
        }

        // Assert
        allOrganizations.Should().NotBeNull();
        allOrganizations.Should().BeAssignableTo<List<Organization>>();
    }

    [Fact]
    public async Task OrganizationsApi_CreateOrganization_WithMinimalData_WorksCorrectly()
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

        // Assert
        createdOrganization.Should().NotBeNull();
        createdOrganization.Id.Should().NotBeNullOrEmpty();
        createdOrganization.Name.Should().Be(organizationName);
        createdOrganization.Slug.Should().Be(organizationSlug);
        createdOrganization.Description.Should().BeNull();
        createdOrganization.WebsiteUrl.Should().BeNull();

        // Cleanup
        try
        {
            await client.Organizations.DeleteAsync(createdOrganization.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OrganizationsApi_UpdateOrganization_WithPartialData_WorksCorrectly()
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

        // Update only the description
        var updateRequest = new OrganizationUpdateRequest
        {
            Description = "Updated description only"
        };

        // Act
        var updatedOrganization = await client.Organizations.UpdateAsync(createdOrganization.Id, updateRequest);

        // Assert
        updatedOrganization.Should().NotBeNull();
        updatedOrganization.Id.Should().Be(createdOrganization.Id);
        updatedOrganization.Name.Should().Be(organizationName); // Should remain unchanged
        updatedOrganization.Description.Should().Be("Updated description only");
        updatedOrganization.WebsiteUrl.Should().Be("https://original.com"); // Should remain unchanged

        // Cleanup
        try
        {
            await client.Organizations.DeleteAsync(updatedOrganization.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OrganizationsApi_ListOrganizations_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var page1 = await client.Organizations.ListAsync(page: 1, limit: 5);
        var page2 = await client.Organizations.ListAsync(page: 2, limit: 5);

        // Assert
        page1.Should().NotBeNull();
        page1.Pagination.Page.Should().Be(1);
        
        page2.Should().NotBeNull();
        page2.Pagination.Page.Should().Be(2);
    }

    [Fact]
    public async Task OrganizationsApi_CreateOrganization_WithDuplicateSlug_ThrowsException()
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

        // Try to create second organization with same slug
        var secondRequest = new OrganizationCreateRequest
        {
            Name = "Second Organization",
            Slug = organizationSlug
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Organizations.CreateAsync(secondRequest));

        // Cleanup
        try
        {
            await client.Organizations.DeleteAsync(firstOrganization.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task OrganizationsApi_UpdateOrganization_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidOrganizationId = "invalid_organization_id";

        var updateRequest = new OrganizationUpdateRequest
        {
            Name = "Updated Name"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Organizations.UpdateAsync(invalidOrganizationId, updateRequest));
    }

    [Fact]
    public async Task OrganizationsApi_DeleteOrganization_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidOrganizationId = "invalid_organization_id";

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Organizations.DeleteAsync(invalidOrganizationId));
    }

    [Fact]
    public async Task OrganizationsApi_ListOrganizations_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Organizations.ListAsync(page: 1, limit: 100);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
    }
}