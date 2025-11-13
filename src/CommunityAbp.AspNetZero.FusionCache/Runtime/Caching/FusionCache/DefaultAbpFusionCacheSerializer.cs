using System.Text.Encodings.Web;
using System.Text.Json;
using Abp.Dependency;
using Abp.Json;
using Abp.Json.SystemTextJson;
using AbpDateTimeConverter = Abp.Json.SystemTextJson.AbpDateTimeConverter;

namespace CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;

/// <summary>
/// Provides the default implementation of a serializer for use with FusionCache in ABP, enabling objects to be
/// serialized to and deserialized from JSON for caching purposes.
/// </summary>
/// <remarks>This serializer uses System.Text.Json with ABP-specific converters to handle serialization
/// and deserialization of cache entries. It is registered as a transient dependency and is intended for use with
/// the ABP FusionCache integration. The serializer wraps cached objects in an AbpCacheData structure to preserve
/// type information during serialization.</remarks>
public class DefaultAbpFusionCacheSerializer : IAbpFusionCacheSerializer, ITransientDependency
{
    /// <summary>
    /// Serializes the specified object to a JSON string, optionally using the provided type information.
    /// </summary>
    /// <param name="value">The object to serialize. If null, the method returns null.</param>
    /// <param name="type">The type to use for serialization. If null, the runtime type of <paramref name="value"/> is used.</param>
    /// <returns>A JSON string representation of the object, or null if <paramref name="value"/> is null.</returns>
    public string? Serialize(object value, Type type = null)
    {
        if (value == null)
        {
            return null;
        }

        var actualType = type ?? value.GetType();
        var cacheData = AbpCacheData.Create(value, actualType);

        return JsonSerializer.Serialize(cacheData, GetJsonSerializerOptions());
    }

    /// <summary>
    /// Deserializes the specified JSON string into an object of the given type or the type specified within the
    /// serialized data.
    /// </summary>
    /// <remarks>If the type parameter is not provided, the method attempts to determine the target
    /// type from metadata within the serialized data. Ensure that the serialized data contains valid type
    /// information if type is omitted.</remarks>
    /// <param name="serializedValue">The JSON string representing the serialized object to deserialize. Cannot be null or empty.</param>
    /// <param name="type">The type to deserialize the object to. If null, the type is determined from the serialized data.</param>
    /// <returns>An object deserialized from the JSON string, or null if the input is null, empty, or cannot be deserialized.</returns>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails due to invalid input or type resolution errors.</exception>
    public object? Deserialize(string serializedValue, Type type = null)
    {
        if (string.IsNullOrEmpty(serializedValue))
        {
            return null;
        }

        try
        {
            var cacheData = JsonSerializer.Deserialize<AbpCacheData>(serializedValue, GetJsonSerializerOptions());

            if (cacheData == null)
            {
                return null;
            }

            var targetType = type ?? Type.GetType(cacheData.Type, true, true);

            return cacheData.Payload.FromJsonString(targetType, GetJsonSerializerOptions());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize cache value: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes the specified string into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="serializedValue">The string containing the serialized representation of the object. Cannot be null.</param>
    /// <returns>An instance of type T that represents the deserialized object, or the default value of T if the input is
    /// null or cannot be deserialized.</returns>
    public T? Deserialize<T>(string serializedValue)
    {
        var result = Deserialize(serializedValue, typeof(T));
        return result == null ? default : (T)result;
    }

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Add ABP-specific converters
        options.Converters.Insert(0, new AbpDateTimeConverter());
        options.Converters.Add(new AbpJsonConverterForType());

        return options;
    }
}
