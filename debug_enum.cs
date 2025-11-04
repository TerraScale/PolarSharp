using System.Text.Json;
using System.Text.Json.Serialization;
using Polar.NET.Models.Products;
using Polar.NET.Extensions;

var json = @"{
    ""type"": ""recurring""
}";

var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverterWithAttributeNames() }
};

try 
{
    var result = JsonSerializer.Deserialize<ProductPrice>(json, options);
    Console.WriteLine($"Success: {result?.Type}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Test the enums directly
var priceTypeJson = "\"recurring\"";
try 
{
    var priceType = JsonSerializer.Deserialize<PriceType>(priceTypeJson, options);
    Console.WriteLine($"PriceType success: {priceType}");
}
catch (Exception ex)
{
    Console.WriteLine($"PriceType error: {ex.Message}");
}

var productPriceTypeJson = "\"fixed\"";
try 
{
    var productPriceType = JsonSerializer.Deserialize<ProductPriceType>(productPriceTypeJson, options);
    Console.WriteLine($"ProductPriceType success: {productPriceType}");
}
catch (Exception ex)
{
    Console.WriteLine($"ProductPriceType error: {ex.Message}");
}