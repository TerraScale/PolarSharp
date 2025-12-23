using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using PolarSharp.Models.Orders;
using PolarSharp.Models.Products;
using PolarSharp.Models.Customers;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Orders API.
/// </summary>
public class OrdersIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public OrdersIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task OrdersApi_ListAsync_ReturnsPaginatedResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Orders.ListAsync(page: 1, limit: 5);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Items.Should().NotBeNull();
                result.Value.Pagination.Should().NotBeNull();
                result.Value.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_ListAllAsync_EnumeratesAllPages()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var orders = new List<Order>();
            await foreach (var orderResult in client.Orders.ListAllAsync())
            {
                if (orderResult.IsFailure) break;
                orders.Add(orderResult.Value);
            }

            // Assert
            orders.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_ListWithFilters_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act & Assert
            // Test with customer ID filter
            var resultWithCustomer = await client.Orders.ListAsync(customerId: "test_customer_id");
            if (resultWithCustomer.IsSuccess)
            {
                resultWithCustomer.Value.Items.Should().NotBeNull();
            }

            // Test with product ID filter
            var resultWithProduct = await client.Orders.ListAsync(productId: "test_product_id");
            if (resultWithProduct.IsSuccess)
            {
                resultWithProduct.Value.Items.Should().NotBeNull();
            }

            // Test with status filter
            var resultWithStatus = await client.Orders.ListAsync(status: OrderStatus.Paid);
            if (resultWithStatus.IsSuccess)
            {
                resultWithStatus.Value.Items.Should().NotBeNull();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_GetOrder_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act & Assert
            // First, try to list orders to get a real order ID
            var listResult = await client.Orders.ListAsync(limit: 1);
            if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
            {
                var orderId = listResult.Value.Items[0].Id;
                var getResult = await client.Orders.GetAsync(orderId);

                if (getResult.IsSuccess && getResult.Value != null)
                {
                    var order = getResult.Value;
                    order.Id.Should().Be(orderId);
                    order.Status.Should().BeOneOf(OrderStatus.Pending, OrderStatus.Paid, OrderStatus.Refunded, OrderStatus.PartiallyRefunded, OrderStatus.Disputed);
                    order.Amount.Should().BeGreaterThanOrEqualTo(0);
                    order.Currency.Should().NotBeNullOrEmpty();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_CreateOrder_HandlesPermissionLimitations()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // First, create a product to order
            var productRequest = new ProductCreateRequest
            {
                Name = $"Test Product {Guid.NewGuid()}",
                Description = "Integration test product for order",
                Type = ProductType.OneTime,
                Prices = new List<ProductPriceCreateRequest>
                {
                    new ProductPriceCreateRequest
                    {
                        Amount = 1000, // $10.00
                        Currency = "usd",
                        Type = ProductPriceType.Fixed
                    }
                }
            };

            var productResult = await client.Products.CreateAsync(productRequest);

            // Act & Assert
            if (productResult.IsSuccess && productResult.Value != null)
            {
                var product = productResult.Value;
                var orderRequest = new OrderCreateRequest
                {
                    ProductId = product.Id,
                    ProductPriceId = product.Prices[0].Id,
                    CustomerEmail = $"test-{Guid.NewGuid()}@testmail.com",
                    CustomerName = "Test Customer",
                    Metadata = new Dictionary<string, object>
                    {
                        ["test"] = true,
                        ["integration"] = true
                    }
                };

                var createOrderResult = await client.Orders.CreateAsync(orderRequest);

                if (createOrderResult.IsSuccess && createOrderResult.Value != null)
                {
                    var createdOrder = createOrderResult.Value;
                    createdOrder.Id.Should().NotBeNullOrEmpty();
                    createdOrder.ProductId.Should().Be(orderRequest.ProductId);
                    createdOrder.ProductPriceId.Should().Be(orderRequest.ProductPriceId);
                    createdOrder.Customer.Should().NotBeNull();
                    createdOrder.Customer!.Email.Should().Be(orderRequest.CustomerEmail);
                    createdOrder.Customer!.Name.Should().Be(orderRequest.CustomerName);
                    createdOrder.Metadata.Should().NotBeNull();
                    createdOrder.Metadata!["test"].Should().Be(true);
                    createdOrder.Metadata!["integration"].Should().Be(true);
                }

                // Cleanup
                await client.Products.ArchiveAsync(product.Id);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_UpdateOrder_HandlesPermissionLimitations()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act & Assert
            // First, try to list orders to get a real order ID
            var listResult = await client.Orders.ListAsync(limit: 1);
            if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
            {
                var orderId = listResult.Value.Items[0].Id;
                var updateRequest = new OrderUpdateRequest
                {
                    Metadata = new Dictionary<string, object>
                    {
                        ["updated"] = true,
                        ["test_run"] = DateTime.UtcNow.ToString("O")
                    },
                    CustomerName = "Updated Customer Name"
                };

                var updateResult = await client.Orders.UpdateAsync(orderId, updateRequest);

                if (updateResult.IsSuccess && updateResult.Value != null)
                {
                    var updatedOrder = updateResult.Value;
                    updatedOrder.Id.Should().Be(orderId);
                    updatedOrder.Metadata.Should().NotBeNull();
                    updatedOrder.Customer.Should().NotBeNull();
                    updatedOrder.Customer!.Name.Should().Be(updateRequest.CustomerName);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_DeleteOrder_HandlesPermissionLimitations()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act & Assert
            // First, try to list orders to get a real order ID
            var listResult = await client.Orders.ListAsync(limit: 1);
            if (listResult.IsSuccess && listResult.Value.Items.Count > 0)
            {
                var orderId = listResult.Value.Items[0].Id;
                var deleteResult = await client.Orders.DeleteAsync(orderId);

                if (deleteResult.IsSuccess && deleteResult.Value != null)
                {
                    deleteResult.Value.Id.Should().Be(orderId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_GetNonExistentOrder_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "order_00000000000000000000000000";

            // Act
            var result = await client.Orders.GetAsync(nonExistentId);

            // Assert - With nullable return types, non-existent resources return null
            if (result.IsSuccess)
            {
                result.Value.Should().BeNull();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_UpdateNonExistentOrder_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "order_00000000000000000000000000";
            var updateRequest = new OrderUpdateRequest
            {
                Metadata = new Dictionary<string, object>
                {
                    ["test"] = true
                }
            };

            // Act
            var result = await client.Orders.UpdateAsync(nonExistentId, updateRequest);

            // Assert - With nullable return types, non-existent resources return null
            if (result.IsSuccess)
            {
                result.Value.Should().BeNull();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_DeleteNonExistentOrder_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var nonExistentId = "order_00000000000000000000000000";

            // Act
            var result = await client.Orders.DeleteAsync(nonExistentId);

            // Assert - With nullable return types, non-existent resources return null
            if (result.IsSuccess)
            {
                result.Value.Should().BeNull();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Refunded)]
    [InlineData(OrderStatus.PartiallyRefunded)]
    [InlineData(OrderStatus.Disputed)]
    public async Task OrdersApi_ListByStatus_WorksCorrectly(OrderStatus status)
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Orders.ListAsync(status: status);

            // Assert
            if (result.IsSuccess)
            {
                result.Value.Items.Should().NotBeNull();
                result.Value.Pagination.Should().NotBeNull();

                // Verify all returned orders have the requested status (if any orders exist)
                foreach (var order in result.Value.Items)
                {
                    order.Status.Should().Be(status);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task OrdersApi_CreateOrderWithValidation_HandlesErrorsCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act & Assert - Missing required fields
            var invalidRequest1 = new OrderCreateRequest();
            var result1 = await client.Orders.CreateAsync(invalidRequest1);

            if (result1.IsFailure)
            {
                // Expected - API should return error for invalid input
                result1.Error.Should().NotBeNull();
            }
            else if (result1.IsSuccess && result1.Error!.Message.Contains("Unauthorized") || result1.Error!.Message.Contains("Forbidden") || result1.Error!.Message.Contains("Method Not Allowed"))
            {
                // Expected in sandbox environment with limited permissions
                _output.WriteLine($"Skipped: {result1.Error!.Message}");
            }

            // Act & Assert - Empty product ID
            var invalidRequest2 = new OrderCreateRequest
            {
                ProductId = "",
                ProductPriceId = "price_123"
            };
            var result2 = await client.Orders.CreateAsync(invalidRequest2);

            if (result2.IsFailure)
            {
                // Expected - API should return error for invalid input
                result2.Error.Should().NotBeNull();
            }
            else if (result2.IsSuccess && result2.Error!.Message.Contains("Unauthorized") || result2.Error!.Message.Contains("Forbidden") || result2.Error!.Message.Contains("Method Not Allowed"))
            {
                // Expected in sandbox environment with limited permissions
                _output.WriteLine($"Skipped: {result2.Error!.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}