using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polar.NET;
using Polar.NET.Models.Products;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var client = PolarClient.Create()
                .WithToken("polar_oat_BArndS0lOEbxHpy7ybDXaVAgQ2b3zVgxxen961iCVZs")
                .WithEnvironment(Polar.NET.Models.Common.PolarEnvironment.Sandbox)
                .Build();

            Console.WriteLine("Testing product creation...");

            var createRequest = new ProductCreateRequest
            {
                Name = $"Test Product {Guid.NewGuid()}",
                Description = "A test product for integration testing",
                Type = ProductType.OneTime,
                Prices = new List<ProductPriceCreateRequest>
                {
                    new ProductPriceCreateRequest
                    {
                        Amount = 1000, // $10.00 in cents
                        Currency = "usd",
                        Type = ProductPriceType.Fixed
                    }
                }
            };

            var createdProduct = await client.Products.CreateAsync(createRequest);
            Console.WriteLine($"Product created successfully! ID: {createdProduct.Id}");
            Console.WriteLine($"Name: {createdProduct.Name}");
            Console.WriteLine($"Type: {createdProduct.Type}");
            Console.WriteLine($"IsRecurring: {createdProduct.IsRecurring}");
            Console.WriteLine($"IsSubscription: {createdProduct.IsSubscription}");

            // Test getting the product
            var retrievedProduct = await client.Products.GetAsync(createdProduct.Id);
            Console.WriteLine($"Product retrieved successfully! Name: {retrievedProduct.Name}");

            // Cleanup
            await client.Products.ArchiveAsync(createdProduct.Id);
            Console.WriteLine("Product archived successfully.");
        }
        catch (PolarApiException ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
            Console.WriteLine($"Status Code: {ex.StatusCode}");
            Console.WriteLine($"Error Type: {ex.ErrorType}");
            if (ex.ResponseBody != null)
            {
                Console.WriteLine($"Response Body: {ex.ResponseBody}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}