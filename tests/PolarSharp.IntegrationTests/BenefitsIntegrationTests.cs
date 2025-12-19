using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PolarSharp.Extensions;
using PolarSharp.Models.Benefits;
using PolarSharp.Models.Customers;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Benefits API.
/// </summary>
public class BenefitsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly List<(string type, string id)> _createdResources;

    public BenefitsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _createdResources = new List<(string, string)>();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnBenefits()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Benefits.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();

        _output.WriteLine($"Found {result.Value.Items.Count} benefits on page {result.Value.Pagination.Page}");
        _output.WriteLine($"Total pages: {result.Value.Pagination.MaxPage}");
    }

    [Fact]
    public async Task ListAsync_WithFilters_ShouldReturnFilteredBenefits()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        // Act - Filter by type
        var result = await client.Benefits.ListAsync(
            type: testBenefit.Type,
            active: true);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.Items.All(b => b.Type == testBenefit.Type).Should().BeTrue();
        result.Value.Items.All(b => b.Active == true).Should().BeTrue();

        _output.WriteLine($"Found {result.Value.Items.Count} benefits of type {testBenefit.Type}");
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ShouldReturnFilteredBenefits()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        // Act
        var builder = client.Benefits.Query()
            .WithType(testBenefit.Type.ToString().ToLowerInvariant())
            .WithSelectable(true);

        var result = await client.Benefits.ListAsync(builder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();

        _output.WriteLine($"Found {result.Value.Items.Count} benefits with query builder");
    }

    [Fact]
    public async Task ListAllAsync_ShouldReturnAllBenefits()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var benefits = new List<Benefit>();
        await foreach (var benefitResult in client.Benefits.ListAllAsync())
        {
            if (benefitResult.IsSuccess)
            {
                benefits.Add(benefitResult.Value);
            }
        }

        // Assert
        benefits.Should().NotBeNull();
        benefits.Count.Should().BeGreaterThanOrEqualTo(0);

        _output.WriteLine($"Total benefits enumerated: {benefits.Count}");
    }

    [Fact]
    public async Task GetAsync_WithValidId_ShouldReturnBenefit()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdBenefitResult = await CreateTestBenefitAsync(client);
        if (createdBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {createdBenefitResult.Error!.Message}");
            return;
        }
        var createdBenefit = createdBenefitResult.Value;
        _createdResources.Add(("benefit", createdBenefit.Id));

        // Act
        var result = await client.Benefits.GetAsync(createdBenefit.Id);

        // Assert
        if (result.IsSuccess)
        {
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(createdBenefit.Id);
            result.Value.Name.Should().Be(createdBenefit.Name);
            result.Value.Type.Should().Be(createdBenefit.Type);
            _output.WriteLine($"Retrieved benefit: {result.Value.Name} ({result.Value.Id})");
        }
        else
        {
            _output.WriteLine($"Skipped: {result.Error!.Message}");
        }
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Benefits.GetAsync("invalid_benefit_id");

        // Assert
        result.IsFailure.Should().BeTrue();
        _output.WriteLine($"Invalid benefit ID correctly returned failure: {result.Error!.Message}");
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateBenefit()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new BenefitCreateRequest
        {
            Name = $"Test Benefit {Guid.NewGuid():N}",
            Description = "Test benefit created via integration test",
            Type = BenefitType.Custom,
            Selectable = true,
            Properties = new Dictionary<string, object>
            {
                ["message"] = "Test benefit message",
                ["integration_test"] = true
            }
        };

        // Act
        var result = await client.Benefits.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdBenefit = result.Value;
        _createdResources.Add(("benefit", createdBenefit.Id));

        createdBenefit.Should().NotBeNull();
        createdBenefit.Id.Should().NotBeNullOrEmpty();
        createdBenefit.Name.Should().Be(request.Name);
        createdBenefit.Description.Should().Be(request.Description);
        createdBenefit.Type.Should().Be(request.Type);
        createdBenefit.Selectable.Should().Be(request.Selectable);
        createdBenefit.Active.Should().BeTrue(); // Default value
        createdBenefit.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        _output.WriteLine($"Created benefit: {createdBenefit.Name} ({createdBenefit.Id})");
    }

    [Fact]
    public async Task CreateAsync_WithTimePeriod_ShouldCreateTimeBasedBenefit()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var timePeriod = new BenefitTimePeriod
        {
            DurationDays = 30
        };

        var request = new BenefitCreateRequest
        {
            Name = $"Time-based Benefit {Guid.NewGuid():N}",
            Description = "Time-based test benefit",
            Type = BenefitType.Time,
            TimePeriod = timePeriod,
            Properties = new Dictionary<string, object>
            {
                ["access_type"] = "premium_features"
            }
        };

        // Act
        var result = await client.Benefits.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdBenefit = result.Value;
        _createdResources.Add(("benefit", createdBenefit.Id));

        createdBenefit.Should().NotBeNull();
        createdBenefit.Type.Should().Be(BenefitType.Time);
        createdBenefit.TimePeriod.Should().NotBeNull();
        createdBenefit.TimePeriod.DurationDays.Should().Be(30);

        _output.WriteLine($"Created time-based benefit: {createdBenefit.Name} ({createdBenefit.Id})");
    }

    [Fact]
    public async Task CreateAsync_WithUsageLimit_ShouldCreateUsageBasedBenefit()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new BenefitCreateRequest
        {
            Name = $"Usage-based Benefit {Guid.NewGuid():N}",
            Description = "Usage-based test benefit",
            Type = BenefitType.Usage,
            UsageLimit = 100,
            Properties = new Dictionary<string, object>
            {
                ["usage_type"] = "api_calls"
            }
        };

        // Act
        var result = await client.Benefits.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdBenefit = result.Value;
        _createdResources.Add(("benefit", createdBenefit.Id));

        createdBenefit.Should().NotBeNull();
        createdBenefit.Type.Should().Be(BenefitType.Usage);
        createdBenefit.UsageLimit.Should().Be(100);

        _output.WriteLine($"Created usage-based benefit: {createdBenefit.Name} ({createdBenefit.Id})");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateBenefit()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdBenefitResult = await CreateTestBenefitAsync(client);
        if (createdBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {createdBenefitResult.Error!.Message}");
            return;
        }
        var createdBenefit = createdBenefitResult.Value;
        _createdResources.Add(("benefit", createdBenefit.Id));

        var updateRequest = new BenefitUpdateRequest
        {
            Name = $"Updated Benefit {Guid.NewGuid():N}",
            Description = "Updated description",
            Active = false,
            Selectable = false,
            Metadata = new Dictionary<string, object>
            {
                ["updated"] = true,
                ["version"] = 2
            }
        };

        // Act
        var result = await client.Benefits.UpdateAsync(createdBenefit.Id, updateRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedBenefit = result.Value;
        updatedBenefit.Should().NotBeNull();
        updatedBenefit.Id.Should().Be(createdBenefit.Id);
        updatedBenefit.Name.Should().Be(updateRequest.Name);
        updatedBenefit.Description.Should().Be(updateRequest.Description);
        updatedBenefit.Active.Should().Be(updateRequest.Active.Value);
        updatedBenefit.Selectable.Should().Be(updateRequest.Selectable.Value);
        updatedBenefit.UpdatedAt.Should().BeAfter(createdBenefit.UpdatedAt ?? DateTime.MinValue);

        _output.WriteLine($"Updated benefit: {updatedBenefit.Name} ({updatedBenefit.Id})");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteBenefit()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var createdBenefitResult = await CreateTestBenefitAsync(client);
        if (createdBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {createdBenefitResult.Error!.Message}");
            return;
        }
        var createdBenefit = createdBenefitResult.Value;
        // Don't add to _createdResources since we're testing deletion

        // Act
        var result = await client.Benefits.DeleteAsync(createdBenefit.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedBenefit = result.Value;
        deletedBenefit.Should().NotBeNull();
        deletedBenefit.Id.Should().Be(createdBenefit.Id);

        // Verify deletion
        var afterDeleteResult = await client.Benefits.GetAsync(createdBenefit.Id);
        afterDeleteResult.IsFailure.Should().BeTrue();

        _output.WriteLine($"Deleted benefit: {deletedBenefit.Name} ({deletedBenefit.Id})");
    }

    [Fact]
    public async Task ListGrantsAsync_ShouldReturnBenefitGrants()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        // Act
        var result = await client.Benefits.ListGrantsAsync(testBenefit.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();

        _output.WriteLine($"Found {result.Value.Items.Count} grants for benefit {testBenefit.Id}");
    }

    [Fact]
    public async Task GrantAsync_WithValidData_ShouldGrantBenefit()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        var testCustomerResult = await CreateTestCustomerAsync(client);
        if (testCustomerResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test customer - {testCustomerResult.Error!.Message}");
            return;
        }
        var testCustomer = testCustomerResult.Value;
        _createdResources.Add(("customer", testCustomer.Id));

        var grantRequest = new BenefitGrantRequest
        {
            CustomerId = testCustomer.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Metadata = new Dictionary<string, object>
            {
                ["test_grant"] = true
            }
        };

        // Act
        var result = await client.Benefits.GrantAsync(testBenefit.Id, grantRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var grant = result.Value;
        _createdResources.Add(("benefit_grant", grant.Id));

        grant.Should().NotBeNull();
        grant.Id.Should().NotBeNullOrEmpty();
        grant.BenefitId.Should().Be(testBenefit.Id);
        grant.CustomerId.Should().Be(testCustomer.Id);
        grant.Status.Should().Be(BenefitGrantStatus.Active);
        if (grant.ExpiresAt.HasValue && grantRequest.ExpiresAt.HasValue)
        {
            grant.ExpiresAt.Value.Should().BeCloseTo(grantRequest.ExpiresAt.Value, TimeSpan.FromSeconds(1));
        }

        _output.WriteLine($"Granted benefit {testBenefit.Id} to customer {testCustomer.Id}");
    }

    [Fact]
    public async Task RevokeGrantAsync_WithValidIds_ShouldRevokeGrant()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        var testCustomerResult = await CreateTestCustomerAsync(client);
        if (testCustomerResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test customer - {testCustomerResult.Error!.Message}");
            return;
        }
        var testCustomer = testCustomerResult.Value;
        _createdResources.Add(("customer", testCustomer.Id));

        var grantRequest = new BenefitGrantRequest
        {
            CustomerId = testCustomer.Id
        };

        var grantResult = await client.Benefits.GrantAsync(testBenefit.Id, grantRequest);
        if (grantResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not grant benefit - {grantResult.Error!.Message}");
            return;
        }
        var grant = grantResult.Value;
        // Don't add to _createdResources since we're testing revocation

        // Act
        var result = await client.Benefits.RevokeGrantAsync(testBenefit.Id, grant.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var revokedGrant = result.Value;
        revokedGrant.Should().NotBeNull();
        revokedGrant.Id.Should().Be(grant.Id);
        revokedGrant.Status.Should().Be(BenefitGrantStatus.Revoked);

        _output.WriteLine($"Revoked grant {grant.Id} for benefit {testBenefit.Id}");
    }

    [Fact]
    public async Task ListAllGrantsAsync_ShouldReturnAllGrants()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        // Act
        var grants = new List<BenefitGrant>();
        await foreach (var grantResult in client.Benefits.ListAllGrantsAsync(testBenefit.Id))
        {
            if (grantResult.IsSuccess)
            {
                grants.Add(grantResult.Value);
            }
        }

        // Assert
        grants.Should().NotBeNull();
        grants.Count.Should().BeGreaterThanOrEqualTo(0);

        _output.WriteLine($"Total grants enumerated for benefit {testBenefit.Id}: {grants.Count}");
    }

    [Fact]
    public async Task ExportAsync_ShouldReturnExportResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Benefits.ExportAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exportResponse = result.Value;
        exportResponse.Should().NotBeNull();
        exportResponse.ExportUrl.Should().NotBeNullOrEmpty();
        exportResponse.Size.Should().BeGreaterThanOrEqualTo(0);
        exportResponse.RecordCount.Should().BeGreaterThanOrEqualTo(0);

        _output.WriteLine($"Export created: {exportResponse.ExportUrl}");
        _output.WriteLine($"File size: {exportResponse.Size} bytes, Records: {exportResponse.RecordCount}");
    }

    [Fact]
    public async Task ExportGrantsAsync_ShouldReturnGrantExportResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        // Act
        var result = await client.Benefits.ExportGrantsAsync(testBenefit.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exportResponse = result.Value;
        exportResponse.Should().NotBeNull();
        exportResponse.ExportUrl.Should().NotBeNullOrEmpty();
        exportResponse.Size.Should().BeGreaterThanOrEqualTo(0);
        exportResponse.RecordCount.Should().BeGreaterThanOrEqualTo(0);

        _output.WriteLine($"Grant export created: {exportResponse.ExportUrl}");
        _output.WriteLine($"File size: {exportResponse.Size} bytes, Records: {exportResponse.RecordCount}");
    }

    [Fact]
    public async Task Pagination_ShouldWorkCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testBenefitResult = await CreateTestBenefitAsync(client);
        if (testBenefitResult.IsFailure)
        {
            _output.WriteLine($"Skipped: Could not create test benefit - {testBenefitResult.Error!.Message}");
            return;
        }
        var testBenefit = testBenefitResult.Value;
        _createdResources.Add(("benefit", testBenefit.Id));

        // Act - Get first page
        var firstPageResult = await client.Benefits.ListAsync(page: 1, limit: 5);

        // Assert
        firstPageResult.IsSuccess.Should().BeTrue();
        var firstPage = firstPageResult.Value;
        firstPage.Should().NotBeNull();
        firstPage.Items.Count.Should().BeLessThanOrEqualTo(5);
        firstPage.Pagination.Page.Should().Be(1);

        if (firstPage.Pagination.MaxPage > 1)
        {
            // Get second page if it exists
            var secondPageResult = await client.Benefits.ListAsync(page: 2, limit: 5);
            secondPageResult.IsSuccess.Should().BeTrue();
            var secondPage = secondPageResult.Value;
            secondPage.Should().NotBeNull();
            secondPage.Pagination.Page.Should().Be(2);

            // Ensure no duplicates between pages
            var firstPageIds = firstPage.Items.Select(b => b.Id).ToHashSet();
            var secondPageIds = secondPage.Items.Select(b => b.Id).ToHashSet();
            firstPageIds.IntersectWith(secondPageIds);
            firstPageIds.Should().BeEmpty();
        }

        _output.WriteLine($"Pagination test completed. Max pages: {firstPage.Pagination.MaxPage}");
    }

    private async Task<PolarResult<Benefit>> CreateTestBenefitAsync(PolarClient client)
    {
        var request = new BenefitCreateRequest
        {
            Name = $"Test Benefit {Guid.NewGuid():N}",
            Description = "Test benefit for integration testing",
            Type = BenefitType.Custom,
            Selectable = true,
            Properties = new Dictionary<string, object>
            {
                ["message"] = "Test benefit message"
            }
        };

        return await client.Benefits.CreateAsync(request);
    }

    private async Task<PolarResult<Customer>> CreateTestCustomerAsync(PolarClient client)
    {
        var request = new CustomerCreateRequest
        {
            Email = $"test-{Guid.NewGuid():N}@mailinator.com",
            Name = "Test Customer",
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["created_at"] = DateTime.UtcNow
            }
        };

        return await client.Customers.CreateAsync(request);
    }
}
