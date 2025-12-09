using Microsoft.Extensions.DependencyInjection;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;

namespace PolarSharp.Examples;

/// <summary>
/// Example demonstrating dependency injection usage for PolarSharp.
/// </summary>
public class DependencyInjectionExample
{
    /// <summary>
    /// Example of how to register and use PolarClient with dependency injection.
    /// </summary>
    public static void Main()
    {
        // Create service collection
        var services = new ServiceCollection();

        // Example 1: Simple registration with access token
        services.AddPolarClient("your-access-token-here");

        // Example 2: Registration with configuration
        services.AddPolarClient(options =>
        {
            options.AccessToken = "your-access-token-here";
            options.Environment = PolarEnvironment.Sandbox;
            options.TimeoutSeconds = 60;
            options.MaxRetryAttempts = 5;
            options.RequestsPerMinute = 250;
            options.UserAgent = "MyApp/1.0.0";
        });

        // Example 3: Sandbox environment helper
        services.AddPolarClientSandbox("your-sandbox-token", options =>
        {
            options.TimeoutSeconds = 45;
            options.MaxRetryAttempts = 3;
        });

        // Example 4: Production environment helper
        services.AddPolarClientProduction("your-production-token", options =>
        {
            options.TimeoutSeconds = 30;
            options.RequestsPerMinute = 300;
        });

        // Example 5: Pre-configured options
        var preConfiguredOptions = new PolarClientOptions
        {
            AccessToken = "your-access-token-here",
            Environment = PolarEnvironment.Sandbox,
            TimeoutSeconds = 90,
            MaxRetryAttempts = 10,
            RequestsPerMinute = 500,
            UserAgent = "MyApp/1.0.0"
        };
        services.AddPolarClient(preConfiguredOptions);

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Resolve and use PolarClient
        using var scope = serviceProvider.CreateScope();
        var polarClient = scope.ServiceProvider.GetRequiredService<IPolarClient>();

        // Now you can use the client
        Console.WriteLine("PolarClient successfully registered and resolved!");
        Console.WriteLine($"Rate limit status: {polarClient.RateLimitStatus}");
        
        // Example usage in a service class
        var productService = new ProductService(polarClient);
        Console.WriteLine("ProductService created with injected PolarClient");
    }
}

/// <summary>
/// Example service class that uses PolarClient via dependency injection.
/// </summary>
public class ProductService
{
    private readonly IPolarClient _polarClient;

    public ProductService(IPolarClient polarClient)
    {
        _polarClient = polarClient;
    }

    /// <summary>
    /// Example method to get all products.
    /// </summary>
    public async Task<List<Models.Products.Product>> GetAllProductsAsync()
    {
        var products = new List<Models.Products.Product>();
        
        await foreach (var product in _polarClient.Products.ListAllAsync())
        {
            products.Add(product);
        }
        
        return products;
    }

    /// <summary>
    /// Example method to create a new product.
    /// </summary>
    public async Task<Models.Products.Product> CreateProductAsync(string name, string description)
    {
        var createRequest = new Models.Products.ProductCreateRequest
        {
            Name = name,
            Description = description,
            Type = Models.Products.ProductType.OneTime
        };

        return await _polarClient.Products.CreateAsync(createRequest);
    }
}