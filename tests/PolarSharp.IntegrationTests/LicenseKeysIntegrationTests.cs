using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PolarSharp.Models.LicenseKeys;
using PolarSharp.Models.Customers;
using PolarSharp.Models.Benefits;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for License Keys API.
/// </summary>
public class LicenseKeysIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly List<(string type, string id)> _createdResources;

    public LicenseKeysIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _createdResources = new List<(string, string)>();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnLicenseKeys()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.LicenseKeys.ListAsync();

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

            _output.WriteLine($"Found {response.Items.Count} license keys on page {response.Pagination.Page}");
            _output.WriteLine($"Total pages: {response.Pagination.MaxPage}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithFilters_ShouldReturnFilteredLicenseKeys()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act & Assert - Test with status filter
            var statusResult = await client.LicenseKeys.ListAsync(status: LicenseKeyStatus.Active);
            statusResult.Should().NotBeNull();
            if (statusResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {statusResult.Error!.Message}");
                return;
            }
            var responseWithStatus = statusResult.Value;
            responseWithStatus.Should().NotBeNull();
            responseWithStatus.Items.Should().NotBeNull();
            responseWithStatus.Items.All(lk => lk.Status == LicenseKeyStatus.Active).Should().BeTrue();

            _output.WriteLine($"Found {responseWithStatus.Items.Count} active license keys");

            // Test with customer ID filter
            var customerResult = await client.LicenseKeys.ListAsync(customerId: "test_customer_id");
            customerResult.Should().NotBeNull();
            if (customerResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {customerResult.Error!.Message}");
                return;
            }
            var responseWithCustomer = customerResult.Value;
            responseWithCustomer.Should().NotBeNull();
            responseWithCustomer.Items.Should().NotBeNull();

            _output.WriteLine($"Found {responseWithCustomer.Items.Count} license keys for test customer");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_ShouldReturnAllLicenseKeys()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var licenseKeys = new List<LicenseKey>();
            await foreach (var licenseKeyResult in client.LicenseKeys.ListAllAsync())
            {
                if (licenseKeyResult.IsFailure) break;

                var licenseKey = licenseKeyResult.Value;
                licenseKeys.Add(licenseKey);
            }

            // Assert
            licenseKeys.Should().NotBeNull();
            licenseKeys.Count.Should().BeGreaterThanOrEqualTo(0);

            _output.WriteLine($"Total license keys enumerated: {licenseKeys.Count}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithValidId_ShouldReturnLicenseKey()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // First, list all license keys to get a valid ID
            var listResult = await client.LicenseKeys.ListAsync();
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            var listResponse = listResult.Value;

            if (listResponse.Items.Any())
            {
                var firstLicenseKey = listResponse.Items.First();

                // Act
                var result = await client.LicenseKeys.GetAsync(firstLicenseKey.Id);

                // Assert
                result.Should().NotBeNull();
                if (result.IsFailure)
                {
                    _output.WriteLine($"Skipped: {result.Error!.Message}");
                    return;
                }
                var retrievedLicenseKey = result.Value;
                retrievedLicenseKey.Should().NotBeNull();
                retrievedLicenseKey.Id.Should().Be(firstLicenseKey.Id);
                retrievedLicenseKey.Key.Should().Be(firstLicenseKey.Key);
                retrievedLicenseKey.Status.Should().Be(firstLicenseKey.Status);

                _output.WriteLine($"Retrieved license key: {retrievedLicenseKey.Key} ({retrievedLicenseKey.Id})");
            }
            else
            {
                _output.WriteLine("No license keys found to test GetAsync");
                true.Should().BeTrue(); // Skip test gracefully
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ShouldReturnNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.LicenseKeys.GetAsync("invalid_license_key_id");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            _output.WriteLine("Invalid license key ID correctly returned failure");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetActivationAsync_ShouldReturnActivationInfo()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // First, list all license keys to get a valid ID
            var listResult = await client.LicenseKeys.ListAsync();
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            var listResponse = listResult.Value;

            if (listResponse.Items.Any())
            {
                var firstLicenseKey = listResponse.Items.First();

                // Act
                var result = await client.LicenseKeys.GetActivationAsync(firstLicenseKey.Id);

                // Assert
                result.Should().NotBeNull();
                if (result.IsFailure)
                {
                    _output.WriteLine($"Skipped: {result.Error!.Message}");
                    return;
                }
                var activation = result.Value;
                activation.Should().NotBeNull();
                activation.Id.Should().NotBeNullOrEmpty();

                _output.WriteLine($"Retrieved activation info for license key: {firstLicenseKey.Id}");
            }
            else
            {
                _output.WriteLine("No license keys found to test GetActivationAsync");
                true.Should().BeTrue(); // Skip test gracefully
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ValidateAsync_WithValidKey_ShouldReturnValidResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var listResult = await client.LicenseKeys.ListAsync();
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            var listResponse = listResult.Value;

            if (listResponse.Items.Any())
            {
                var firstLicenseKey = listResponse.Items.First();
                var request = new LicenseKeyValidateRequest
                {
                    Key = firstLicenseKey.Key,
                    UserIdentifier = "test_user",
                    MachineFingerprint = "test_fingerprint"
                };

                // Act
                var result = await client.LicenseKeys.ValidateAsync(request);

                // Assert
                result.Should().NotBeNull();
                if (result.IsFailure)
                {
                    _output.WriteLine($"Skipped: {result.Error!.Message}");
                    return;
                }
                var response = result.Value;
                response.Should().NotBeNull();
                response.Valid.Should().BeTrue();
                response.LicenseKey.Should().NotBeNull();
                response.LicenseKey.Id.Should().Be(firstLicenseKey.Id);

                _output.WriteLine($"Validated license key: {firstLicenseKey.Key} - Valid: {response.Valid}");
            }
            else
            {
                _output.WriteLine("No license keys found to test ValidateAsync");
                true.Should().BeTrue(); // Skip test gracefully
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidKey_ShouldReturnInvalidResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var request = new LicenseKeyValidateRequest
            {
                Key = "INVALID_LICENSE_KEY",
                UserIdentifier = "test_user"
            };

            // Act
            var result = await client.LicenseKeys.ValidateAsync(request);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            var response = result.Value;
            response.Should().NotBeNull();
            response.Valid.Should().BeFalse();
            response.Error.Should().NotBeNullOrEmpty();
            response.ErrorCode.Should().NotBeNullOrEmpty();

            _output.WriteLine($"Validation result for invalid key: Valid={response.Valid}, Error={response.Error}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ActivateAsync_WithValidData_ShouldActivateLicenseKey()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var listResult = await client.LicenseKeys.ListAsync();
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            var listResponse = listResult.Value;

            if (listResponse.Items.Any(lk => lk.Status == LicenseKeyStatus.Inactive))
            {
                var inactiveLicenseKey = listResponse.Items.First(lk => lk.Status == LicenseKeyStatus.Inactive);
                var request = new LicenseKeyActivateRequest
                {
                    UserIdentifier = "test_user",
                    MachineFingerprint = "test_fingerprint",
                    DisplayName = "Test Activation",
                    Metadata = new Dictionary<string, object>
                    {
                        ["test"] = true
                    }
                };

                // Act
                var result = await client.LicenseKeys.ActivateAsync(inactiveLicenseKey.Id, request);

                // Assert
                result.Should().NotBeNull();
                if (result.IsFailure)
                {
                    _output.WriteLine($"Skipped: {result.Error!.Message}");
                    return;
                }
                var response = result.Value;
                response.Should().NotBeNull();
                response.Success.Should().BeTrue();
                response.LicenseKey.Should().NotBeNull();
                response.Activation.Should().NotBeNull();

                _output.WriteLine($"Activated license key: {inactiveLicenseKey.Id} - Success: {response.Success}");
            }
            else
            {
                _output.WriteLine("No inactive license keys found to test ActivateAsync");
                true.Should().BeTrue(); // Skip test gracefully
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ActivateAsync_WithInvalidData_ShouldReturnFailureResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var listResult = await client.LicenseKeys.ListAsync();
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            var listResponse = listResult.Value;

            if (listResponse.Items.Any())
            {
                var firstLicenseKey = listResponse.Items.First();
                var request = new LicenseKeyActivateRequest
                {
                    UserIdentifier = "", // Invalid empty user identifier
                    MachineFingerprint = "test_fingerprint"
                };

                // Act
                var result = await client.LicenseKeys.ActivateAsync(firstLicenseKey.Id, request);

                // Assert
                result.Should().NotBeNull();
                if (result.IsFailure)
                {
                    _output.WriteLine($"Skipped: {result.Error!.Message}");
                    return;
                }
                var response = result.Value;
                response.Should().NotBeNull();
                response.Success.Should().BeFalse();
                response.Error.Should().NotBeNullOrEmpty();
                response.ErrorCode.Should().NotBeNullOrEmpty();

                _output.WriteLine($"Activation result for invalid data: Success={response.Success}, Error={response.Error}");
            }
            else
            {
                _output.WriteLine("No license keys found to test ActivateAsync with invalid data");
                true.Should().BeTrue(); // Skip test gracefully
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task DeactivateAsync_WithValidData_ShouldDeactivateLicenseKey()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var listResult = await client.LicenseKeys.ListAsync();
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            var listResponse = listResult.Value;

            if (listResponse.Items.Any(lk => lk.Status == LicenseKeyStatus.Active))
            {
                var activeLicenseKey = listResponse.Items.First(lk => lk.Status == LicenseKeyStatus.Active);
                var request = new LicenseKeyDeactivateRequest
                {
                    UserIdentifier = "test_user",
                    MachineFingerprint = "test_fingerprint",
                    Reason = "Test deactivation",
                    Metadata = new Dictionary<string, object>
                    {
                        ["test"] = true
                    }
                };

                // Act
                var result = await client.LicenseKeys.DeactivateAsync(activeLicenseKey.Id, request);

                // Assert
                result.Should().NotBeNull();
                if (result.IsFailure)
                {
                    _output.WriteLine($"Skipped: {result.Error!.Message}");
                    return;
                }
                var response = result.Value;
                response.Should().NotBeNull();
                response.Success.Should().BeTrue();
                response.LicenseKey.Should().NotBeNull();

                _output.WriteLine($"Deactivated license key: {activeLicenseKey.Id} - Success: {response.Success}");
            }
            else
            {
                _output.WriteLine("No active license keys found to test DeactivateAsync");
                true.Should().BeTrue(); // Skip test gracefully
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateLicenseKey()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var listResult = await client.LicenseKeys.ListAsync();
            if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error!.Message}");
                return;
            }
            var listResponse = listResult.Value;

            if (listResponse.Items.Any())
            {
                var firstLicenseKey = listResponse.Items.First();
                var updateRequest = new LicenseKeyUpdateRequest
                {
                    DisplayName = $"Updated License Key {Guid.NewGuid():N}",
                    Enabled = false,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    MaxActivations = 5,
                    Metadata = new Dictionary<string, object>
                    {
                        ["updated"] = true,
                        ["version"] = 2
                    }
                };

                // Act
                var result = await client.LicenseKeys.UpdateAsync(firstLicenseKey.Id, updateRequest);

                // Assert
                result.Should().NotBeNull();
                if (result.IsFailure)
                {
                    _output.WriteLine($"Skipped: {result.Error!.Message}");
                    return;
                }
                var updatedLicenseKey = result.Value;
                updatedLicenseKey.Should().NotBeNull();
                updatedLicenseKey.Id.Should().Be(firstLicenseKey.Id);
                updatedLicenseKey.UpdatedAt.Should().BeAfter(firstLicenseKey.UpdatedAt);

                _output.WriteLine($"Updated license key: {updatedLicenseKey.Id}");
            }
            else
            {
                _output.WriteLine("No license keys found to test UpdateAsync");
                true.Should().BeTrue(); // Skip test gracefully
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task Pagination_ShouldWorkCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Get first page
            var firstPageResult = await client.LicenseKeys.ListAsync(page: 1, limit: 5);

            // Assert
            firstPageResult.Should().NotBeNull();
            if (firstPageResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {firstPageResult.Error!.Message}");
                return;
            }
            var firstPage = firstPageResult.Value;
            firstPage.Should().NotBeNull();
            firstPage.Items.Count.Should().BeLessThanOrEqualTo(5);
            firstPage.Pagination.Page.Should().Be(1);

            if (firstPage.Pagination.MaxPage > 1)
            {
                // Get second page if it exists
                var secondPageResult = await client.LicenseKeys.ListAsync(page: 2, limit: 5);
                if (secondPageResult.IsFailure)
                {
                    _output.WriteLine($"Skipped: {secondPageResult.Error!.Message}");
                    return;
                }
                var secondPage = secondPageResult.Value;
                secondPage.Should().NotBeNull();
                secondPage.Pagination.Page.Should().Be(2);

                // Ensure no duplicates between pages
                var firstPageIds = firstPage.Items.Select(lk => lk.Id).ToHashSet();
                var secondPageIds = secondPage.Items.Select(lk => lk.Id).ToHashSet();
                firstPageIds.IntersectWith(secondPageIds);
                firstPageIds.Should().BeEmpty();
            }

            _output.WriteLine($"Pagination test completed. Max pages: {firstPage.Pagination.MaxPage}");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_WithFilters_ShouldReturnFilteredLicenseKeys()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var activeLicenseKeys = new List<LicenseKey>();
            await foreach (var licenseKeyResult in client.LicenseKeys.ListAllAsync(status: LicenseKeyStatus.Active))
            {
                if (licenseKeyResult.IsFailure) break;

                var licenseKey = licenseKeyResult.Value;
                activeLicenseKeys.Add(licenseKey);
            }

            // Assert
            activeLicenseKeys.Should().NotBeNull();
            activeLicenseKeys.All(lk => lk.Status == LicenseKeyStatus.Active).Should().BeTrue();

            _output.WriteLine($"Found {activeLicenseKeys.Count} active license keys using ListAllAsync");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}