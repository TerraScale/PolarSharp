using PolarSharp;
using PolarSharp.Models.Products;
using PolarSharp.Models.Customers;
using PolarSharp.Models.Benefits;
using PolarSharp.Models.Orders;
using PolarSharp.Models.Subscriptions;
using PolarSharp.Models.Checkouts;
using PolarSharp.Models.LicenseKeys;
using PolarSharp.Models.Discounts;
using PolarSharp.Models.Webhooks;
using PolarSharp.Models.Files;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Utility class to clean up the sandbox environment before running tests.
/// </summary>
public class SandboxCleanup
{
    private readonly PolarClient _client;
    private readonly ITestOutputHelper _output;

    public SandboxCleanup(PolarClient client, ITestOutputHelper output)
    {
        _client = client;
        _output = output;
    }

    /// <summary>
    /// Cleans up all resources in the sandbox environment.
    /// </summary>
    public async Task CleanupAllResourcesAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Starting sandbox cleanup...");
        
        try
        {
            await CleanupProductsAsync(cancellationToken);
            await CleanupCustomersAsync(cancellationToken);
            await CleanupBenefitsAsync(cancellationToken);
            await CleanupOrdersAsync(cancellationToken);
            await CleanupSubscriptionsAsync(cancellationToken);
            await CleanupCheckoutsAsync(cancellationToken);
            await CleanupLicenseKeysAsync(cancellationToken);
            await CleanupDiscountsAsync(cancellationToken);
            await CleanupWebhooksAsync(cancellationToken);
            await CleanupFilesAsync(cancellationToken);
            
            _output.WriteLine("Sandbox cleanup completed successfully.");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Cleanup was cancelled due to timeout.");
            throw;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error during cleanup: {ex.Message}");
            throw;
        }
    }

    private async Task CleanupProductsAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Cleaning up products...");
        
        try
        {
            var products = new List<Product>();
            await foreach (var productResult in _client.Products.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (productResult.IsSuccess)
                {
                    products.Add(productResult.Value);
                }
            }

            foreach (var product in products)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _client.Products.ArchiveAsync(product.Id);
                    _output.WriteLine($"Archived product: {product.Id} - {product.Name}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to archive product {product.Id}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up products: {ex.Message}");
        }
    }

    private async Task CleanupCustomersAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Cleaning up customers...");
        
        try
        {
            var customers = new List<Customer>();
            await foreach (var customerResult in _client.Customers.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (customerResult.IsSuccess)
                {
                    customers.Add(customerResult.Value);
                }
            }

            foreach (var customer in customers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _client.Customers.DeleteAsync(customer.Id);
                    _output.WriteLine($"Deleted customer: {customer.Id} - {customer.Email}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to delete customer {customer.Id}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up customers: {ex.Message}");
        }
    }

    private async Task CleanupBenefitsAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Cleaning up benefits...");
        
        try
        {
            var benefits = new List<Benefit>();
            await foreach (var benefitResult in _client.Benefits.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (benefitResult.IsSuccess)
                {
                    benefits.Add(benefitResult.Value);
                }
            }

            foreach (var benefit in benefits)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _client.Benefits.DeleteAsync(benefit.Id);
                    _output.WriteLine($"Deleted benefit: {benefit.Id} - {benefit.Name}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to delete benefit {benefit.Id}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up benefits: {ex.Message}");
        }
    }

    private async Task CleanupOrdersAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Checking orders (read-only)...");
        
        try
        {
            var count = 0;
            await foreach (var orderResult in _client.Orders.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (orderResult.IsSuccess) count++;
                if (count >= 10) break; // Limit to avoid rate limiting
            }

            _output.WriteLine($"Found at least {count} orders (cannot be deleted)");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking orders: {ex.Message}");
        }
    }

    private async Task CleanupSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Checking subscriptions (read-only)...");
        
        try
        {
            var count = 0;
            await foreach (var subscriptionResult in _client.Subscriptions.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (subscriptionResult.IsSuccess) count++;
                if (count >= 10) break; // Limit to avoid rate limiting
            }

            _output.WriteLine($"Found at least {count} subscriptions (cannot be deleted)");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking subscriptions: {ex.Message}");
        }
    }

    private async Task CleanupCheckoutsAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Checking checkouts (read-only)...");
        
        try
        {
            var count = 0;
            await foreach (var checkoutResult in _client.Checkouts.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (checkoutResult.IsSuccess) count++;
                if (count >= 10) break; // Limit to avoid rate limiting
            }

            _output.WriteLine($"Found at least {count} checkouts (cannot be deleted)");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking checkouts: {ex.Message}");
        }
    }

    private async Task CleanupLicenseKeysAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Checking license keys (read-only)...");
        
        try
        {
            var count = 0;
            await foreach (var licenseKeyResult in _client.LicenseKeys.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (licenseKeyResult.IsSuccess) count++;
                if (count >= 10) break; // Limit to avoid rate limiting
            }

            _output.WriteLine($"Found at least {count} license keys (cannot be deleted)");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking license keys: {ex.Message}");
        }
    }

    private async Task CleanupDiscountsAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Cleaning up discounts...");
        
        try
        {
            var discounts = new List<PolarSharp.Models.Discounts.Discount>();
            await foreach (var discountResult in _client.Discounts.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (discountResult.IsSuccess)
                {
                    discounts.Add(discountResult.Value);
                }
            }

            foreach (var discount in discounts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _client.Discounts.DeleteAsync(discount.Id);
                    _output.WriteLine($"Deleted discount: {discount.Id} - {discount.Name}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to delete discount {discount.Id}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up discounts: {ex.Message}");
        }
    }

    private async Task CleanupWebhooksAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Cleaning up webhooks...");
        
        try
        {
            var page = 1;
            var hasMore = true;
            var webhooks = new List<PolarSharp.Models.Webhooks.WebhookEndpoint>();

            while (hasMore)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var response = await _client.Webhooks.ListEndpointsAsync(page: page, limit: 100);
                if (response.IsSuccess)
                {
                    webhooks.AddRange(response.Value.Items);
                    hasMore = response.Value.Pagination.Page < response.Value.Pagination.MaxPage;
                    page++;
                }
                else
                {
                    hasMore = false;
                }
            }

            foreach (var webhook in webhooks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _client.Webhooks.DeleteEndpointAsync(webhook.Id);
                    _output.WriteLine($"Deleted webhook: {webhook.Id} - {webhook.Url}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to delete webhook {webhook.Id}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up webhooks: {ex.Message}");
        }
    }

    private async Task CleanupFilesAsync(CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Checking files (read-only)...");
        
        try
        {
            var count = 0;
            await foreach (var fileResult in _client.Files.ListAllAsync().WithCancellation(cancellationToken))
            {
                if (fileResult.IsSuccess) count++;
                if (count >= 10) break; // Limit to avoid rate limiting
            }

            _output.WriteLine($"Found at least {count} files (cannot be deleted)");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking files: {ex.Message}");
        }
    }
}