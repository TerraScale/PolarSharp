using System.Text.Json;
using Polar.NET.Models.Products;

var request = new ProductCreateRequest
{
    Name = "Test Product",
    Type = ProductType.OneTime,
    Prices = new List<ProductPriceCreateRequest>
    {
        new ProductPriceCreateRequest
        {
            Amount = 999,
            Currency = "USD",
            Type = ProductPriceType.OneTime
        }
    }
};

var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
{ 
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
});

Console.WriteLine(json);