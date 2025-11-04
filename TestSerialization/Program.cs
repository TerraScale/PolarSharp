using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Polar.NET.Extensions;
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
            Type = ProductPriceType.Fixed
        }
    }
};

var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
{ 
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Converters = { new JsonStringEnumConverterWithAttributeNames() }
});

Console.WriteLine(json);
