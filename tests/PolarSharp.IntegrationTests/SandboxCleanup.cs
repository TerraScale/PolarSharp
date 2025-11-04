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
    public async Task CleanupAllResourcesAsync()
    {
        _output.WriteLine("Starting sandbox cleanup...");
        
        try
        {
            await CleanupProductsAsync();
            await CleanupCustomersAsync();
            await CleanupBenefitsAsync();
            await CleanupOrdersAsync();
            await CleanupSubscriptionsAsync();
            await CleanupCheckoutsAsync();
            await CleanupLicenseKeysAsync();
            await CleanupDiscountsAsync();
            await CleanupWebhooksAsync();
            await CleanupFilesAsync();
            
            _output.WriteLine("Sandbox cleanup completed successfully.");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error during cleanup: {ex.Message}");
            throw;
        }
    }

    private async Task CleanupProductsAsync()
    {
        _output.WriteLine("Cleaning up products...");
        
        try
        {
            var products = new List<Product>();
            await foreach (var product in _client.Products.ListAllAsync())
            {
                products.Add(product);
            }

            foreach (var product in products)
            {
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
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up products: {ex.Message}");
        }
    }

    private async Task CleanupCustomersAsync()
    {
        _output.WriteLine("Cleaning up customers...");
        
        try
        {
            var customers = new List<Customer>();
            await foreach (var customer in _client.Customers.ListAllAsync())
            {
                customers.Add(customer);
            }

            foreach (var customer in customers)
            {
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
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up customers: {ex.Message}");
        }
    }

    private async Task CleanupBenefitsAsync()
    {
        _output.WriteLine("Cleaning up benefits...");
        
        try
        {
            var benefits = new List<Benefit>();
            await foreach (var benefit in _client.Benefits.ListAllAsync())
            {
                benefits.Add(benefit);
            }

            foreach (var benefit in benefits)
            {
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
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up benefits: {ex.Message}");
        }
    }

    private async Task CleanupOrdersAsync()
    {
        _output.WriteLine("Checking orders (read-only)...");
        
        try
        {
            var orders = new List<PolarSharp.Models.Orders.Order>();
            await foreach (var order in _client.Orders.ListAllAsync())
            {
                orders.Add(order);
            }
            
            _output.WriteLine($"Found {orders.Count} orders (cannot be deleted)");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking orders: {ex.Message}");
        }
    }

    private async Task CleanupSubscriptionsAsync()
    {
        _output.WriteLine("Checking subscriptions (read-only)...");
        
        try
        {
            var subscriptions = new List<PolarSharp.Models.Subscriptions.Subscription>();
            await foreach (var subscription in _client.Subscriptions.ListAllAsync())
            {
                subscriptions.Add(subscription);
            }
            
            _output.WriteLine($"Found {subscriptions.Count} subscriptions (cannot be deleted)");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking subscriptions: {ex.Message}");
        }
    }

    private async Task CleanupCheckoutsAsync()
    {
        _output.WriteLine("Checking checkouts (read-only)...");
        
        try
        {
            var checkouts = new List<PolarSharp.Models.Checkouts.Checkout>();
            await foreach (var checkout in _client.Checkouts.ListAllAsync())
            {
                checkouts.Add(checkout);
            }
            
            _output.WriteLine($"Found {checkouts.Count} checkouts (cannot be deleted)");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking checkouts: {ex.Message}");
        }
    }

    private async Task CleanupLicenseKeysAsync()
    {
        _output.WriteLine("Checking license keys (read-only)...");
        
        try
        {
            var licenseKeys = new List<PolarSharp.Models.LicenseKeys.LicenseKey>();
            await foreach (var licenseKey in _client.LicenseKeys.ListAllAsync())
            {
                licenseKeys.Add(licenseKey);
            }
            
            _output.WriteLine($"Found {licenseKeys.Count} license keys (cannot be deleted)");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking license keys: {ex.Message}");
        }
    }

    private async Task CleanupDiscountsAsync()
    {
        _output.WriteLine("Cleaning up discounts...");
        
        try
        {
            var discounts = new List<PolarSharp.Models.Discounts.Discount>();
            await foreach (var discount in _client.Discounts.ListAllAsync())
            {
                discounts.Add(discount);
            }

            foreach (var discount in discounts)
            {
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
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up discounts: {ex.Message}");
        }
    }

    private async Task CleanupWebhooksAsync()
    {
        _output.WriteLine("Cleaning up webhooks...");
        
        try
        {
            var page = 1;
            var hasMore = true;
            var webhooks = new List<PolarSharp.Models.Webhooks.WebhookEndpoint>();

            while (hasMore)
            {
                var response = await _client.Webhooks.ListEndpointsAsync(page: page, limit: 100);
                webhooks.AddRange(response.Items);
                hasMore = response.Pagination.Page < response.Pagination.MaxPage;
                page++;
            }

            foreach (var webhook in webhooks)
            {
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
        catch (Exception ex)
        {
            _output.WriteLine($"Error cleaning up webhooks: {ex.Message}");
        }
    }

    private async Task CleanupFilesAsync()
    {
        _output.WriteLine("Checking files (read-only)...");
        
        try
        {
            var files = new List<PolarSharp.Models.Files.File>();
            await foreach (var file in _client.Files.ListAllAsync())
            {
                files.Add(file);
            }
            
            _output.WriteLine($"Found {files.Count} files (cannot be deleted)");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error checking files: {ex.Message}");
        }
    }
}