using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace Polar.NET.Extensions;

/// <summary>
/// A custom JSON string enum converter that uses JsonPropertyName attributes on enum values.
/// </summary>
public class JsonStringEnumConverterWithAttributeNames : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <returns>true if the converter can convert the specified type; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A converter for the specified type.</returns>
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

            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
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