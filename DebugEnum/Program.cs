using System.Text.Json;
using System.Text.Json.Serialization;
// using Polar.NET.Models.Products;
// using Polar.NET.Extensions;

public enum PriceType
{
    [JsonPropertyName("one_time")]
    OneTime,
    
    [JsonPropertyName("recurring")]
    Recurring
}

public enum ProductPriceType
{
    [JsonPropertyName("fixed")]
    Fixed,
    
    [JsonPropertyName("custom")]
    Custom,
    
    [JsonPropertyName("free")]
    Free,
    
    [JsonPropertyName("seat_based")]
    SeatBased,
    
    [JsonPropertyName("metered_unit")]
    MeteredUnit
}

public class TestPrice
{
    [JsonPropertyName("type")]
    public PriceType Type { get; set; }
    
    [JsonPropertyName("amount_type")]
    public ProductPriceType AmountType { get; set; }
}

var json = @"{
    ""type"": ""recurring"",
    ""amount_type"": ""fixed""
}";

var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverterWithAttributeNames() }
};

try 
{
    var result = JsonSerializer.Deserialize<TestPrice>(json, options);
    Console.WriteLine($"Success: Type={result?.Type}, AmountType={result?.AmountType}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
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

public class JsonStringEnumConverterWithAttributeNames : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(
            typeof(EnumConverter<>).MakeGenericType(typeToConvert))!;
    }

    private class EnumConverter<T> : JsonConverter<T> where T : Enum
    {
        private readonly Dictionary<T, string> _enumToString;
        private readonly Dictionary<string, T> _stringToEnum;

        public EnumConverter()
        {
            _enumToString = new Dictionary<T, string>();
            _stringToEnum = new Dictionary<string, T>();

            foreach (var field in typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                var enumValue = (T)field.GetValue(null)!;
                var jsonPropertyName = field.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
                var name = jsonPropertyName ?? enumValue.ToString();
                
                _enumToString[enumValue] = name;
                _stringToEnum[name] = enumValue;
            }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null || !_stringToEnum.TryGetValue(value, out var result))
            {
                throw new JsonException($"Unable to convert \"{value}\" to enum {typeof(T).Name}.");
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(_enumToString[value]);
        }
    }
}
