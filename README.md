# PolarSharp

A highly efficient and easy-to-use REST API client for the [Polar.sh](https://polar.sh) payments platform. This .NET library provides comprehensive access to Polar's Core API for server-side operations like managing products, orders, subscriptions, checkouts, benefits, customer sessions, and license keys.

## Features

- 🚀 **High Performance**: Built with `IHttpClientFactory`, async/await, and efficient memory usage
- 🔄 **Automatic Retries**: Exponential backoff retry policy for transient failures and rate limits
- 🛡️ **Rate Limiting**: Built-in rate limiting to respect API limits (300 req/min default)
- 🔧 **Fluent API**: Clean, expressive builder pattern for configuration and queries
- 📄 **Full Pagination**: `IAsyncEnumerable` support for efficient enumeration of all results
- 🎯 **Type Safe**: Complete strongly-typed models and requests
- 📝 **Comprehensive**: Full API coverage for all Polar Core endpoints
- 🔐 **Secure**: Bearer token authentication with proper header management
- 🧪 **Tested**: Comprehensive integration and unit test coverage
- 📦 **NuGet Ready**: Packaged with symbols and source linking

## Installation

```bash
dotnet add package PolarSharp
```

## Quick Start

### Basic Usage

```csharp
using PolarSharp;

// Initialize the client
var client = PolarClient.Create()
    .WithToken("your-access-token")
    .WithEnvironment(PolarEnvironment.Sandbox)
    .Build();

// List products
var products = await client.Products.ListAsync(limit: 10);
foreach (var product in products.Items)
{
    Console.WriteLine($"Product: {product.Name} - {product.Description}");
}
```

### Dependency Injection (Recommended)

```csharp
// In your DI setup (Program.cs or Startup.cs)
services.AddPolarClient("your-access-token");

// Or with configuration
services.AddPolarClient(options =>
{
    options.AccessToken = "your-access-token";
    options.Environment = PolarEnvironment.Sandbox;
    options.TimeoutSeconds = 60;
    options.MaxRetryAttempts = 5;
    options.RequestsPerMinute = 250;
});

// Or for specific environments
services.AddPolarClientSandbox("your-sandbox-token");
services.AddPolarClientProduction("your-production-token");

// In your service
public class ProductService
{
    private readonly IPolarClient _client;
    
    public ProductService(IPolarClient client)
    {
        _client = client;
    }
    
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();
        await foreach (var product in _client.Products.ListAllAsync())
        {
            products.Add(product);
        }
        return products;
    }
}
```

### Using IHttpClientFactory (Manual)

```csharp
// In your DI setup
services.AddHttpClient<PolarClient>(client =>
{
    client.BaseAddress = new Uri("https://api.polar.sh/v1");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", "your-access-token");
});

// In your service
public class ProductService
{
    private readonly PolarClient _client;
    
    public ProductService(PolarClient client)
    {
        _client = client;
    }
    
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();
        await foreach (var product in _client.Products.ListAllAsync())
        {
            products.Add(product);
        }
        return products;
    }
}
```

## API Coverage

### Products

```csharp
// List products with filtering
var products = await client.Products.ListAsync(page: 1, limit: 20);

// Advanced filtering with query builder
var filteredProducts = await client.Products.ListAsync(
    client.Products.Query()
        .WithActive(true)
        .WithType("one_time")
        .CreatedAfter(DateTime.UtcNow.AddDays(-30)),
    limit: 50
);

// Get a specific product
var product = await client.Products.GetAsync("product_id");

// Create a new product
var newProduct = await client.Products.CreateAsync(new ProductCreateRequest
{
    Name = "My Software License",
    Description = "Perpetual license for my software",
    Type = ProductType.OneTime
});

// Update a product
var updatedProduct = await client.Products.UpdateAsync("product_id", new ProductUpdateRequest
{
    Name = "Updated Product Name",
    Description = "Updated description"
});

// Archive a product
await client.Products.ArchiveAsync("product_id");

// Create a price for a product
var price = await client.Products.CreatePriceAsync("product_id", new ProductPriceCreateRequest
{
    Amount = 1999, // $19.99
    Currency = "USD",
    Type = ProductPriceType.OneTime
});

// Enumerate all products efficiently
await foreach (var product in client.Products.ListAllAsync())
{
    Console.WriteLine($"Processing product: {product.Name}");
}
```

### Orders

```csharp
// List orders
var orders = await client.Orders.ListAsync();

// Filter orders with query builder
var customerOrders = await client.Orders.ListAsync(
    client.Orders.Query()
        .WithCustomerId("customer_123")
        .WithStatus("paid")
        .CreatedAfter(DateTime.UtcNow.AddDays(-7))
);

// Get a specific order
var order = await client.Orders.GetAsync("order_id");

// Create an order
var newOrder = await client.Orders.CreateAsync(new OrderCreateRequest
{
    CustomerId = "customer_123",
    ProductId = "product_123",
    ProductPriceId = "price_123"
});

// Update an order
var updatedOrder = await client.Orders.UpdateAsync("order_id", new OrderUpdateRequest
{
    Metadata = new Dictionary<string, object>
    {
        ["internal_order_id"] = "ORD-001"
    }
});
```

### Subscriptions

```csharp
// List subscriptions
var subscriptions = await client.Subscriptions.ListAsync();

// Filter subscriptions
var activeSubscriptions = await client.Subscriptions.ListAsync(
    client.Subscriptions.Query()
        .WithStatus("active")
        .WithCustomerId("customer_123")
);

// Get a specific subscription
var subscription = await client.Subscriptions.GetAsync("subscription_id");

// Create a subscription
var newSubscription = await client.Subscriptions.CreateAsync(new SubscriptionCreateRequest
{
    CustomerId = "customer_123",
    ProductId = "product_123",
    ProductPriceId = "price_123"
});

// Cancel a subscription
await client.Subscriptions.CancelAsync("subscription_id");

// Update a subscription
var updatedSubscription = await client.Subscriptions.UpdateAsync("subscription_id", new SubscriptionUpdateRequest
{
    Metadata = new Dictionary<string, object>
    {
        ["tier"] = "premium"
    }
});
```

### Checkouts

```csharp
// List checkouts
var checkouts = await client.Checkouts.ListAsync();

// Create a checkout session
var checkout = await client.Checkouts.CreateAsync(new CheckoutCreateRequest
{
    ProductId = "product_123",
    ProductPriceId = "price_123",
    SuccessUrl = "https://yourapp.com/success",
    CancelUrl = "https://yourapp.com/cancel",
    CustomerEmail = "customer@example.com"
});

// Get a specific checkout
var existingCheckout = await client.Checkouts.GetAsync("checkout_id");

// Update a checkout
var updatedCheckout = await client.Checkouts.UpdateAsync("checkout_id", new CheckoutUpdateRequest
{
    Metadata = new Dictionary<string, object>
    {
        ["campaign"] = "summer_sale"
    }
});
```

### Benefits

```csharp
// List benefits
var benefits = await client.Benefits.ListAsync();

// Create a benefit
var newBenefit = await client.Benefits.CreateAsync(new BenefitCreateRequest
{
    Name = "Premium Support",
    Description = "Access to premium customer support",
    Type = BenefitType.Custom,
    Selectable = true,
    Properties = new Dictionary<string, object>
    {
        ["response_time"] = "2h",
        ["channels"] = new[] { "email", "chat" }
    }
});

// Get a specific benefit
var benefit = await client.Benefits.GetAsync("benefit_id");

// Update a benefit
var updatedBenefit = await client.Benefits.UpdateAsync("benefit_id", new BenefitUpdateRequest
{
    Name = "Updated Benefit Name",
    Description = "Updated description"
});

// Delete a benefit
await client.Benefits.DeleteAsync("benefit_id");
```

### Customers

```csharp
// List customers
var customers = await client.Customers.ListAsync();

// Filter customers with query builder
var recentCustomers = await client.Customers.ListAsync(
    client.Customers.Query()
        .CreatedAfter(DateTime.UtcNow.AddDays(-30))
        .WithEmail("@company.com")
);

// Get a specific customer
var customer = await client.Customers.GetAsync("customer_id");

// Create a customer
var newCustomer = await client.Customers.CreateAsync(new CustomerCreateRequest
{
    Email = "customer@example.com",
    Name = "John Doe",
    ExternalId = "user_123",
    Metadata = new Dictionary<string, object>
    {
        ["source"] = "website",
        ["plan"] = "premium"
    }
});

// Update a customer
var updatedCustomer = await client.Customers.UpdateAsync("customer_id", new CustomerUpdateRequest
{
    Name = "Jane Doe",
    Metadata = new Dictionary<string, object>
    {
        ["last_updated_by"] = "admin"
    }
});

// Delete a customer
await client.Customers.DeleteAsync("customer_id");
```

### Customer Sessions

```csharp
// Create a customer session for portal access
var session = await client.CustomerSessions.CreateAsync(new CustomerSessionCreateRequest
{
    CustomerId = "customer_123"
});

// Introspect a customer session
var introspection = await client.CustomerSessions.IntrospectAsync(new CustomerSessionIntrospectRequest
{
    CustomerAccessToken = session.CustomerAccessToken
});
```

### License Keys

```csharp
// List license keys
var licenseKeys = await client.LicenseKeys.ListAsync();

// Create a license key
var newLicenseKey = await client.LicenseKeys.CreateAsync(new LicenseKeyCreateRequest
{
    CustomerId = "customer_123",
    BenefitId = "benefit_123",
    Metadata = new Dictionary<string, object>
    {
        ["version"] = "2.0.0",
        ["features"] = new[] { "pro", "api_access" }
    }
});

// Validate a license key
var validation = await client.LicenseKeys.ValidateAsync(new LicenseKeyValidateRequest
{
    Key = "LICENSE-KEY-123"
});

if (validation.Valid)
{
    Console.WriteLine($"License is valid for: {validation.LicenseKey?.Customer?.Name}");
}

// Activate a license key
var activation = await client.LicenseKeys.ActivateAsync("license_key_id", new LicenseKeyActivateRequest
{
    Device = "User's Device Name",
    Metadata = new Dictionary<string, object>
    {
        ["ip_address"] = "192.168.1.1"
    }
});

// Deactivate a license key
var deactivation = await client.LicenseKeys.DeactivateAsync("license_key_id", new LicenseKeyDeactivateRequest
{
    Reason = "Device decommissioned"
});
```

### Customer Portal API

For customer-facing operations using customer access tokens:

```csharp
// Create customer portal API client
var customerApi = CustomerPortalApi.CreateSandbox("customer_access_token");

// Get current customer
var customer = await customerApi.GetCustomerAsync();

// Update customer information
var updatedCustomer = await customerApi.UpdateCustomerAsync(new CustomerUpdateRequest
{
    Name = "Updated Name"
});

// List customer's orders
var customerOrders = await customerApi.ListOrdersAsync();

// List customer's subscriptions
var customerSubscriptions = await customerApi.ListSubscriptionsAsync();

// Cancel a subscription
await customerApi.CancelSubscriptionAsync("subscription_id");

// List customer's benefit grants
var benefitGrants = await customerApi.ListBenefitGrantsAsync();

// List customer's license keys
var customerLicenseKeys = await customerApi.ListLicenseKeysAsync();

// Validate a license key
var validation = await customerApi.ValidateLicenseKeyAsync(new LicenseKeyValidateRequest
{
    Key = "LICENSE-KEY-123"
});
```

## Configuration

### Client Options

```csharp
var client = PolarClient.Create()
    .WithToken("your-access-token")
    .WithEnvironment(PolarEnvironment.Production)
    .WithTimeout(60)                    // Timeout in seconds
    .WithMaxRetries(5)                  // Max retry attempts
    .WithUserAgent("MyApp/1.0.0")       // Custom user agent
    .Build();
```

### Advanced Configuration

```csharp
var options = new PolarClientOptions
{
    AccessToken = "your-access-token",
    Environment = PolarEnvironment.Sandbox,
    TimeoutSeconds = 45,
    MaxRetryAttempts = 5,
    InitialRetryDelayMs = 2000,
    RequestsPerMinute = 250,
    UserAgent = "MyApp/1.0.0",
    JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    }
};

var client = new PolarClient(options);
```

## Error Handling

PolarSharp provides comprehensive error handling with custom exceptions:

```csharp
try
{
    var product = await client.Products.GetAsync("invalid_id");
}
catch (PolarApiException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Error Type: {ex.ErrorType}");
    
    if (ex.Details.HasValue)
    {
        Console.WriteLine($"Error Details: {ex.Details.Value}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Environments

- **Production**: `https://api.polar.sh/v1` (Live transactions)
- **Sandbox**: `https://sandbox-api.polar.sh/v1` (Testing, no real charges)

## Rate Limiting

PolarSharp includes built-in rate limiting to respect API limits:

- Default: 300 requests per minute
- Configurable via `PolarClientOptions.RequestsPerMinute`
- Automatic exponential backoff for rate limit responses (429)

## Pagination

All list endpoints support pagination:

```csharp
// Manual pagination
var page1 = await client.Products.ListAsync(page: 1, limit: 50);
var page2 = await client.Products.ListAsync(page: 2, limit: 50);

// Automatic enumeration with IAsyncEnumerable
await foreach (var product in client.Products.ListAllAsync())
{
    // Process each product
    Console.WriteLine(product.Name);
}
```

## Authentication

PolarSharp uses Bearer token authentication. You can create access tokens in your Polar dashboard:

1. Go to [Polar.sh](https://polar.sh)
2. Navigate to Settings → API Keys
3. Create a new Organization Access Token (OAT)
4. Use the token in your client initialization

## Dependencies

- .NET 8.0 or .NET 9.0
- Microsoft.Extensions.Http
- Microsoft.Extensions.Http.Polly
- Polly
- System.Text.Json
- System.ComponentModel.Annotations

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📖 [Documentation](https://docs.polar.sh)
- 🐛 [Issues](https://github.com/mariogk/PolarSharp/issues)
- 💬 [Discord](https://discord.gg/Pnhfz3UThd)
- 📧 [Email](mailto:support@polar.sh)

## Related Projects

- [Polar TypeScript SDK](https://github.com/polarsource/polar)
- [Polar Python SDK](https://github.com/polarsource/polar-python)
- [Polar Go SDK](https://github.com/polarsource/polar-go)