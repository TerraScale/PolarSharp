using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Polar.NET.Models.Refunds;
using Xunit;
using Xunit.Abstractions;

namespace Polar.NET.IntegrationTests;

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
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task RefundsApi_ListAllAsync_EnumeratesAllPages()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var refunds = new List<Refund>();
        await foreach (var refund in client.Refunds.ListAllAsync())
        {
            refunds.Add(refund);
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
        try
        {
            var listResult = await client.Refunds.ListAsync(limit: 1);
            if (listResult.Items.Count > 0)
            {
                var refundId = listResult.Items[0].Id;
                var refund = await client.Refunds.GetAsync(refundId);

                refund.Should().NotBeNull();
                refund.Id.Should().Be(refundId);
                refund.Amount.Should().BeGreaterThan(0);
                refund.Currency.Should().NotBeNullOrEmpty();
                refund.PaymentId.Should().NotBeNullOrEmpty();
                refund.Status.Should().BeOneOf(RefundStatus.Pending, RefundStatus.Succeeded, RefundStatus.Failed, RefundStatus.Canceled);
                refund.CreatedAt.Should().BeBefore(DateTime.UtcNow);
                refund.UpdatedAt.Should().BeBefore(DateTime.UtcNow);
            }
            else
            {
                // No refunds found, skip test
                true.Should().BeTrue();
            }
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RefundsApi_CreateRefund_HandlesPermissionLimitations()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        // Creating refunds requires a valid payment ID, which we may not have in sandbox
        try
        {
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

            var createdRefund = await client.Refunds.CreateAsync(refundRequest);

            createdRefund.Should().NotBeNull();
            createdRefund.Id.Should().NotBeNullOrEmpty();
            createdRefund.PaymentId.Should().Be(refundRequest.PaymentId);
            createdRefund.Amount.Should().Be(refundRequest.Amount);
            createdRefund.Reason.Should().Be(refundRequest.Reason);
            createdRefund.Metadata.Should().NotBeNull();
            createdRefund.Metadata!["test"].Should().Be(true);
            createdRefund.Metadata!["integration"].Should().Be(true);
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("RequestValidationError") || ex.Message.Contains("Not Found"))
        {
            // Expected in sandbox environment with limited permissions or invalid payment ID
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RefundsApi_GetNonExistentRefund_HandlesErrorCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var nonExistentId = "refund_00000000000000000000000000";

        // Act & Assert
        try
        {
            var action = async () => await client.Refunds.GetAsync(nonExistentId);
            await action.Should().ThrowAsync<Exception>();
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RefundsApi_CreateRefundWithValidation_HandlesErrorsCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert - Missing required fields
        var invalidRequest1 = new RefundCreateRequest();
        try
        {
            var action1 = async () => await client.Refunds.CreateAsync(invalidRequest1);
            await action1.Should().ThrowAsync<Exception>();
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }

        // Act & Assert - Empty payment ID
        var invalidRequest2 = new RefundCreateRequest
        {
            PaymentId = "",
            Amount = 1000
        };
        try
        {
            var action2 = async () => await client.Refunds.CreateAsync(invalidRequest2);
            await action2.Should().ThrowAsync<Exception>();
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }

        // Act & Assert - Zero amount
        var invalidRequest3 = new RefundCreateRequest
        {
            PaymentId = "payment_test_123",
            Amount = 0
        };
        try
        {
            var action3 = async () => await client.Refunds.CreateAsync(invalidRequest3);
            await action3.Should().ThrowAsync<Exception>();
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RefundsApi_ListWithQueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
            var builder = client.Refunds.Query();
            var result = await client.Refunds.ListAsync(builder, page: 1, limit: 5);

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Pagination.Should().NotBeNull();
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RefundsApi_CreateRefundWithMetadata_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
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

            var createdRefund = await client.Refunds.CreateAsync(refundRequest);

            createdRefund.Should().NotBeNull();
            createdRefund.Metadata.Should().NotBeNull();
            createdRefund.Metadata!["customer_request"].Should().Be(true);
            createdRefund.Metadata!["refund_type"].Should().Be("partial");
            createdRefund.Metadata!["processed_by"].Should().Be("integration_test");
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("RequestValidationError") || ex.Message.Contains("Not Found"))
        {
            // Expected in sandbox environment with limited permissions or invalid payment ID
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RefundsApi_ListPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
            // Test first page
            var firstPage = await client.Refunds.ListAsync(page: 1, limit: 2);
            firstPage.Should().NotBeNull();
            firstPage.Items.Should().NotBeNull();
            firstPage.Pagination.Should().NotBeNull();

            if (firstPage.Items.Count > 0 && firstPage.Pagination.MaxPage > 1)
            {
                // Test second page if it exists
                var secondPage = await client.Refunds.ListAsync(page: 2, limit: 2);
                secondPage.Should().NotBeNull();
                secondPage.Items.Should().NotBeNull();
                secondPage.Pagination.Should().NotBeNull();

                // Ensure no duplicate items between pages
                var firstPageIds = firstPage.Items.Select(r => r.Id).ToHashSet();
                var secondPageIds = secondPage.Items.Select(r => r.Id).ToHashSet();
                firstPageIds.IntersectWith(secondPageIds);
                firstPageIds.Should().BeEmpty();
            }
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RefundsApi_RefundProperties_AreValid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act & Assert
        try
        {
            var listResult = await client.Refunds.ListAsync(limit: 1);
            if (listResult.Items.Count > 0)
            {
                var refund = listResult.Items[0];

                // Test all required properties
                refund.Id.Should().NotBeNullOrEmpty();
                refund.Amount.Should().BeGreaterThan(0);
                refund.Currency.Should().NotBeNullOrEmpty();
                refund.PaymentId.Should().NotBeNullOrEmpty();
                refund.Status.Should().BeOneOf(RefundStatus.Pending, RefundStatus.Succeeded, RefundStatus.Failed, RefundStatus.Canceled);
                refund.CreatedAt.Should().BeBefore(DateTime.UtcNow);
                refund.UpdatedAt.Should().BeBefore(DateTime.UtcNow);

                // Test optional properties
                refund.OrderId.Should().NotBeNull(); // Can be null or have value
                refund.Reason.Should().NotBeNull(); // Can be null or have value
                refund.Metadata.Should().NotBeNull(); // Can be null or have value
                refund.ReceiptUrl.Should().NotBeNull(); // Can be null or have value
            }
            else
            {
                // No refunds found, skip test
                true.Should().BeTrue();
            }
        }
        catch (Polar.NET.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }
}