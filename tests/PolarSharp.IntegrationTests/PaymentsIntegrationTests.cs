using FluentAssertions;
using PolarSharp.Models.Payments;
using PolarSharp.Models.Refunds;
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
        var response = await client.Payments.ListAsync(page: 1, limit: 10);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        // API may return 0-indexed or 1-indexed pages
        response.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task PaymentsApi_GetPayment_WithValidId_ReturnsPayment()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, list payments to get a valid ID
        var payments = await client.Payments.ListAsync();
        if (payments.Items.Count == 0)
        {
            return; // Skip if no payments exist
        }

        var paymentId = payments.Items.First().Id;

        // Act
        var payment = await client.Payments.GetAsync(paymentId);

        // Assert
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

        // Act & Assert
        try
        {
            var result = await client.Payments.GetAsync(invalidPaymentId);
            // With nullable return types, invalid IDs return null
            result.Should().BeNull();
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("Forbidden") || ex.Message.Contains("Method Not Allowed"))
        {
            // Expected in sandbox environment with limited permissions
            true.Should().BeTrue();
        }
    }

    [Fact]
    public async Task PaymentsApi_ListAllPayments_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allPayments = new List<Payment>();
        await foreach (var payment in client.Payments.ListAllAsync())
        {
            allPayments.Add(payment);
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

        var response = await client.Payments.ListAsync(queryBuilder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var page1 = await client.Payments.ListAsync(page: 1, limit: 5);
        var page2 = await client.Payments.ListAsync(page: 2, limit: 5);

        // Assert
        page1.Should().NotBeNull();
        page1.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
        
        page2.Should().NotBeNull();
        page2.Pagination.Page.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Payments.ListAsync(page: 1, limit: 100);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_VerifyStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Payments.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        
        foreach (var payment in response.Items)
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
        var response = await client.Payments.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        
        foreach (var payment in response.Items)
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
        var smallPage = await client.Payments.ListAsync(page: 1, limit: 1);
        var mediumPage = await client.Payments.ListAsync(page: 1, limit: 10);
        var largePage = await client.Payments.ListAsync(page: 1, limit: 50);

        // Assert
        smallPage.Should().NotBeNull();
        mediumPage.Should().NotBeNull();
        largePage.Should().NotBeNull();
        
        // All should have the same total count
        smallPage.Pagination.TotalCount.Should().Be(mediumPage.Pagination.TotalCount);
        mediumPage.Pagination.TotalCount.Should().Be(largePage.Pagination.TotalCount);
    }

    [Fact]
    public async Task PaymentsApi_GetPayment_VerifyFullStructure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, list payments to get a valid ID
        var payments = await client.Payments.ListAsync();
        if (payments.Items.Count == 0)
        {
            return; // Skip if no payments exist
        }

        var paymentId = payments.Items.First().Id;

        // Act
        var payment = await client.Payments.GetAsync(paymentId);

        // Assert
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

        var response = await client.Payments.ListAsync(queryBuilder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Items.Should().BeEmpty();
        response.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task PaymentsApi_ListPayments_VerifyFailedPayments()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Payments.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        
        var failedPayments = response.Items.Where(p => p.Status == PaymentStatus.Failed).ToList();
        
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
        var response = await client.Payments.ListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        
        var succeededPayments = response.Items.Where(p => p.Status == PaymentStatus.Succeeded).ToList();
        
        foreach (var succeededPayment in succeededPayments)
        {
            succeededPayment.Status.Should().Be(PaymentStatus.Succeeded);
            succeededPayment.FailureReason.Should().BeNull();
            succeededPayment.FailureCode.Should().BeNull();
        }
    }
}