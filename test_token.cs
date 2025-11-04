using System;
using System.Threading.Tasks;
using Polar.NET;

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

            // Test a simple read operation
            var products = await client.Products.ListAsync(page: 1, limit: 1);
            Console.WriteLine($"API Token is valid. Found {products.TotalCount} products.");
        }
        catch (PolarApiException ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
            if (ex.ErrorResponse != null)
            {
                Console.WriteLine($"Error Type: {ex.ErrorResponse.Type}");
                Console.WriteLine($"Error Detail: {ex.ErrorResponse.Detail}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}