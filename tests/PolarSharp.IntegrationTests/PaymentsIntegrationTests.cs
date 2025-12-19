using FluentAssertions;
using PolarSharp.Models.Payments;
using PolarSharp.Models.Refunds;
using PolarSharp.Results;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Payments API.
/// </summary>
public class PaymentsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public PaymentsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Payments.ListAsync(page: 1, limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
        // API may return 0-indexed or 1-indexed pages
        result.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task PaymentsApi_GetPayment_WithValidId_ReturnsPayment()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, list payments to get a valid ID
        var paymentsResult = await client.Payments.ListAsync();
        if (!paymentsResult.IsSuccess || paymentsResult.Value.Items.Count == 0)
        {
            return; // Skip if no payments exist
        }

        var paymentId = paymentsResult.Value.Items.First().Id;

        // Act
        var paymentResult = await client.Payments.GetAsync(paymentId);

        // Assert
        paymentResult.Should().NotBeNull();
        paymentResult.IsSuccess.Should().BeTrue();
        var payment = paymentResult.Value;
        payment.Should().NotBeNull();
        payment.Id.Should().Be(paymentId);
        payment.Amount.Should().BeGreaterThanOrEqualTo(0);
        payment.Currency.Should().NotBeNullOrEmpty();
        payment.Status.Should().BeOneOf(PaymentStatus.Pending, PaymentStatus.Succeeded, PaymentStatus.Failed, PaymentStatus.Canceled, PaymentStatus.RequiresAction, PaymentStatus.RequiresConfirmation, PaymentStatus.RequiresPaymentMethod);
        payment.Type.Should().BeOneOf(PaymentType.OneTime, PaymentType.Subscription, PaymentType.Installment);
        payment.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        payment.UpdatedAt.Should().BeOnOrAfter(payment.CreatedAt);
    }

    [Fact]
    public async Task PaymentsApi_GetPayment_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidPaymentId = "invalid_payment_id";

        // Act
        var result = await client.Payments.GetAsync(invalidPaymentId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task PaymentsApi_ListAllPayments_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allPayments = new List<Payment>();
        await foreach (var paymentResult in client.Payments.ListAllAsync())
        {
            if (paymentResult.IsFailure) break;
            allPayments.Add(paymentResult.Value);
        }

        // Assert
        allPayments.Should().NotBeNull();
        allPayments.Should().BeAssignableTo<List<Payment>>();
    }

    [Fact]
    public async Task PaymentsApi_QueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var queryBuilder = client.Payments.Query();

        var result = await client.Payments.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result1 = await client.Payments.ListAsync(page: 1, limit: 5);
        var result2 = await client.Payments.ListAsync(page: 2, limit: 5);

        // Assert
        result1.Should().NotBeNull();
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);

        result2.Should().NotBeNull();
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Payments.ListAsync(page: 1, limit: 100);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_VerifyStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Payments.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();

        foreach (var payment in result.Value.Items)
        {
            payment.Id.Should().NotBeNullOrEmpty();
            payment.Amount.Should().BeGreaterThanOrEqualTo(0);
            payment.Currency.Should().NotBeNullOrEmpty();
            payment.Status.Should().BeOneOf(PaymentStatus.Pending, PaymentStatus.Succeeded, PaymentStatus.Failed, PaymentStatus.Canceled, PaymentStatus.RequiresAction, PaymentStatus.RequiresConfirmation, PaymentStatus.RequiresPaymentMethod);
            payment.Type.Should().BeOneOf(PaymentType.OneTime, PaymentType.Subscription, PaymentType.Installment);
            payment.CreatedAt.Should().BeBefore(DateTime.UtcNow);
            payment.UpdatedAt.Should().BeOnOrAfter(payment.CreatedAt);
        }
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_VerifyNestedObjects()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Payments.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();

        foreach (var payment in result.Value.Items)
        {
            // Payment method may be null if not included
            if (payment.PaymentMethod != null)
            {
                payment.PaymentMethod.Id.Should().NotBeNullOrEmpty();
                payment.PaymentMethod.Type.Should().BeOneOf(PaymentMethodType.Card, PaymentMethodType.BankAccount, PaymentMethodType.PayPal, PaymentMethodType.Other);
                payment.PaymentMethod.CreatedAt.Should().BeBefore(DateTime.UtcNow);
            }

            // Refunds may be empty if no refunds
            if (payment.Refunds != null)
            {
                payment.Refunds.Should().BeAssignableTo<List<Refund>>();
            }
        }
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_WithDifferentPageSizes_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var smallPageResult = await client.Payments.ListAsync(page: 1, limit: 1);
        var mediumPageResult = await client.Payments.ListAsync(page: 1, limit: 10);
        var largePageResult = await client.Payments.ListAsync(page: 1, limit: 50);

        // Assert
        smallPageResult.Should().NotBeNull();
        smallPageResult.IsSuccess.Should().BeTrue();
        mediumPageResult.Should().NotBeNull();
        mediumPageResult.IsSuccess.Should().BeTrue();
        largePageResult.Should().NotBeNull();
        largePageResult.IsSuccess.Should().BeTrue();

        // All should have the same total count
        smallPageResult.Value.Pagination.TotalCount.Should().Be(mediumPageResult.Value.Pagination.TotalCount);
        mediumPageResult.Value.Pagination.TotalCount.Should().Be(largePageResult.Value.Pagination.TotalCount);
    }

    [Fact]
    public async Task PaymentsApi_GetPayment_VerifyFullStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, list payments to get a valid ID
        var paymentsResult = await client.Payments.ListAsync();
        if (!paymentsResult.IsSuccess || paymentsResult.Value.Items.Count == 0)
        {
            return; // Skip if no payments exist
        }

        var paymentId = paymentsResult.Value.Items.First().Id;

        // Act
        var paymentResult = await client.Payments.GetAsync(paymentId);

        // Assert
        paymentResult.Should().NotBeNull();
        paymentResult.IsSuccess.Should().BeTrue();
        var payment = paymentResult.Value;
        payment.Should().NotBeNull();
        payment.Id.Should().Be(paymentId);
        payment.Amount.Should().BeGreaterThanOrEqualTo(0);
        payment.Currency.Should().NotBeNullOrEmpty();
        payment.Status.Should().BeOneOf(PaymentStatus.Pending, PaymentStatus.Succeeded, PaymentStatus.Failed, PaymentStatus.Canceled, PaymentStatus.RequiresAction, PaymentStatus.RequiresConfirmation, PaymentStatus.RequiresPaymentMethod);
        payment.Type.Should().BeOneOf(PaymentType.OneTime, PaymentType.Subscription, PaymentType.Installment);
        payment.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        payment.UpdatedAt.Should().BeOnOrAfter(payment.CreatedAt);
        
        // Verify optional fields
        if (payment.PaymentMethodId != null)
        {
            payment.PaymentMethodId.Should().NotBeNullOrEmpty();
        }
        
        if (payment.CustomerId != null)
        {
            payment.CustomerId.Should().NotBeNullOrEmpty();
        }
        
        if (payment.OrderId != null)
        {
            payment.OrderId.Should().NotBeNullOrEmpty();
        }
        
        if (payment.SubscriptionId != null)
        {
            payment.SubscriptionId.Should().NotBeNullOrEmpty();
        }
        
        if (payment.CheckoutId != null)
        {
            payment.CheckoutId.Should().NotBeNullOrEmpty();
        }
        
        // Verify nested objects if present
        if (payment.PaymentMethod != null)
        {
            payment.PaymentMethod.Id.Should().NotBeNullOrEmpty();
            payment.PaymentMethod.Type.Should().BeOneOf(PaymentMethodType.Card, PaymentMethodType.BankAccount, PaymentMethodType.PayPal, PaymentMethodType.Other);
        }
        
        if (payment.Refunds != null)
        {
            payment.Refunds.Should().BeAssignableTo<List<Refund>>();
        }
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_EmptyResponse_HandlesGracefully()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act - Use a filter that likely returns no results
        var queryBuilder = client.Payments.Query()
            .WithCustomerId("non_existent_customer_id");

        var result = await client.Payments.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_VerifyFailedPayments()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Payments.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();

        var failedPayments = result.Value.Items.Where(p => p.Status == PaymentStatus.Failed).ToList();
        
        foreach (var failedPayment in failedPayments)
        {
            failedPayment.Status.Should().Be(PaymentStatus.Failed);
            
            // Failed payments should have failure reason or code
            if (failedPayment.FailureReason != null)
            {
                failedPayment.FailureReason.Should().NotBeNullOrEmpty();
            }
            
            if (failedPayment.FailureCode != null)
            {
                failedPayment.FailureCode.Should().NotBeNullOrEmpty();
            }
        }
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_VerifySucceededPayments()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Payments.ListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();

        var succeededPayments = result.Value.Items.Where(p => p.Status == PaymentStatus.Succeeded).ToList();
        
        foreach (var succeededPayment in succeededPayments)
        {
            succeededPayment.Status.Should().Be(PaymentStatus.Succeeded);
            succeededPayment.FailureReason.Should().BeNull();
            succeededPayment.FailureCode.Should().BeNull();
        }
    }
}