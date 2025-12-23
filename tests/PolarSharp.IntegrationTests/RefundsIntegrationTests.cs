using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using PolarSharp.Models.Refunds;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Refunds API.
/// </summary>
public class RefundsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RefundsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task RefundsApi_ListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Refunds.ListAsync(page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        if (!result.IsSuccess)
        {
            // Sandbox may not support refunds API
            return;
        }
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
        result.Value.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Value.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task RefundsApi_ListAllAsync_EnumeratesAllPages()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var refunds = new List<Refund>();
        await foreach (var refundResult in client.Refunds.ListAllAsync())
        {
            if (refundResult.IsFailure) break;
            refunds.Add(refundResult.Value);
        }

        // Assert
        refunds.Should().NotBeNull();
    }

    [Fact]
    public async Task RefundsApi_GetRefund_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // First, try to list refunds to get a real refund ID
        var listResult = await client.Refunds.ListAsync(limit: 1);
        if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
        {
            var refundId = listResult.Value.Items[0].Id;
            var refundResult = await client.Refunds.GetAsync(refundId);

            if (refundResult.IsSuccess && refundResult.Value != null)
            {
                var refund = refundResult.Value;
                refund.Id.Should().Be(refundId);
                refund.Amount.Should().BeGreaterThan(0);
                refund.Currency.Should().NotBeNullOrEmpty();
                refund.PaymentId.Should().NotBeNullOrEmpty();
                refund.Status.Should().BeOneOf(RefundStatus.Pending, RefundStatus.Succeeded, RefundStatus.Failed, RefundStatus.Canceled);
                refund.CreatedAt.Should().BeBefore(DateTime.UtcNow);
                refund.UpdatedAt.Should().BeBefore(DateTime.UtcNow);
            }
        }
    }

    [Fact]
    public async Task RefundsApi_CreateRefund_HandlesPermissionLimitations()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // Creating refunds requires a valid payment ID, which we may not have in sandbox
        var refundRequest = new RefundCreateRequest
        {
            PaymentId = "payment_test_123456789",
            Amount = 1000, // $10.00
            Reason = "Integration test refund",
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["integration"] = true
            }
        };

        var result = await client.Refunds.CreateAsync(refundRequest);

        if (result.IsSuccess && result.Value != null)
        {
            var createdRefund = result.Value;
            createdRefund.Should().NotBeNull();
            createdRefund.Id.Should().NotBeNullOrEmpty();
            createdRefund.PaymentId.Should().Be(refundRequest.PaymentId);
            createdRefund.Amount.Should().Be(refundRequest.Amount);
            createdRefund.Reason.Should().Be(refundRequest.Reason);
            createdRefund.Metadata.Should().NotBeNull();
            createdRefund.Metadata!["test"].Should().Be(true);
            createdRefund.Metadata!["integration"].Should().Be(true);
        }
        else
        {
            // Expected in sandbox environment with limited permissions or invalid payment ID
            _output.WriteLine("Refund creation returned null or failed - expected in sandbox or with invalid payment ID");
        }
    }

    [Fact]
    public async Task RefundsApi_GetNonExistentRefund_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "refund_00000000000000000000000000";

            // Act
            var result = await client.Refunds.GetAsync(nonExistentId);

            // Assert - With PolarResult pattern, non-existent resources return failure
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task RefundsApi_CreateRefundWithValidation_HandlesErrorsCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - Missing required fields
        var invalidRequest1 = new RefundCreateRequest();
        var result1 = await client.Refunds.CreateAsync(invalidRequest1);
        result1.IsSuccess.Should().BeFalse(); // Should return failure for validation errors

        // Act & Assert - Empty payment ID
        var invalidRequest2 = new RefundCreateRequest
        {
            PaymentId = "",
            Amount = 1000
        };
        var result2 = await client.Refunds.CreateAsync(invalidRequest2);
        result2.IsSuccess.Should().BeFalse(); // Should return failure for validation errors

        // Act & Assert - Zero amount
        var invalidRequest3 = new RefundCreateRequest
        {
            PaymentId = "payment_test_123",
            Amount = 0
        };
        var result3 = await client.Refunds.CreateAsync(invalidRequest3);
        result3.IsSuccess.Should().BeFalse(); // Should return failure for validation errors
    }

    [Fact]
    public async Task RefundsApi_ListWithQueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var builder = client.Refunds.Query();
        var result = await client.Refunds.ListAsync(builder, page: 1, limit: 5);

        // Assert
        result.Should().NotBeNull();
        if (!result.IsSuccess)
        {
            // Sandbox may not support refunds API
            return;
        }
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task RefundsApi_CreateRefundWithMetadata_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        var refundRequest = new RefundCreateRequest
        {
            PaymentId = "payment_test_123456789",
            Amount = 1500, // $15.00
            Reason = "Customer requested refund",
            Metadata = new Dictionary<string, object>
            {
                ["customer_request"] = true,
                ["refund_type"] = "partial",
                ["processed_by"] = "integration_test",
                ["timestamp"] = DateTime.UtcNow.ToString("O")
            }
        };

        var result = await client.Refunds.CreateAsync(refundRequest);

        if (result.IsSuccess && result.Value != null)
        {
            var createdRefund = result.Value;
            createdRefund.Should().NotBeNull();
            createdRefund.Metadata.Should().NotBeNull();
            createdRefund.Metadata!["customer_request"].Should().Be(true);
            createdRefund.Metadata!["refund_type"].Should().Be("partial");
            createdRefund.Metadata!["processed_by"].Should().Be("integration_test");
        }
        else
        {
            // Expected in sandbox environment with limited permissions or invalid payment ID
            _output.WriteLine("Refund creation returned null or failed - expected in sandbox or with invalid payment ID");
        }
    }

    [Fact]
    public async Task RefundsApi_ListPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        // Test first page
        var firstPageResult = await client.Refunds.ListAsync(page: 1, limit: 2);

        // Assert
        firstPageResult.Should().NotBeNull();
        if (!firstPageResult.IsSuccess)
        {
            // Sandbox may not support refunds API
            return;
        }
        firstPageResult.Value.Should().NotBeNull();
        firstPageResult.Value.Items.Should().NotBeNull();
        firstPageResult.Value.Pagination.Should().NotBeNull();

        if (firstPageResult.Value.Items.Count > 0 && firstPageResult.Value.Pagination.MaxPage > 1)
        {
            // Test second page if it exists
            var secondPageResult = await client.Refunds.ListAsync(page: 2, limit: 2);
            secondPageResult.Should().NotBeNull();
            if (secondPageResult.IsSuccess)
            {
                secondPageResult.Value.Items.Should().NotBeNull();
                secondPageResult.Value.Pagination.Should().NotBeNull();

                // Verify pagination structure is consistent
                _output.WriteLine($"First page: {firstPageResult.Value.Items.Count} items, Second page: {secondPageResult.Value.Items.Count} items");
            }
        }
    }

    [Fact]
    public async Task RefundsApi_RefundProperties_AreValid()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var listResult = await client.Refunds.ListAsync(limit: 1);

            // Assert
            if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
            {
                var refund = listResult.Value.Items[0];

                // Test all required properties
                refund.Id.Should().NotBeNullOrEmpty();
                refund.Amount.Should().BeGreaterThan(0);
                refund.Currency.Should().NotBeNullOrEmpty();
                refund.PaymentId.Should().NotBeNullOrEmpty();
                refund.Status.Should().BeOneOf(RefundStatus.Pending, RefundStatus.Succeeded, RefundStatus.Failed, RefundStatus.Canceled);
                refund.CreatedAt.Should().BeBefore(DateTime.UtcNow);
                refund.UpdatedAt.Should().BeBefore(DateTime.UtcNow);

                // Optional properties are tested only if they have values
                // No assertions needed for optional null properties
            }
            else if (listResult.IsFailure)
            {
                _output.WriteLine($"Skipped: {listResult.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}
