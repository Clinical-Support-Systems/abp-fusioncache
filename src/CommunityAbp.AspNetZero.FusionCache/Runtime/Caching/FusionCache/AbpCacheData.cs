using System.Text.Json;

namespace CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;

/// <summary>
/// Wraps cached data with type information for proper deserialization.
/// </summary>
public class AbpCacheData
{
    public string? Type { get; set; }
    public string? Payload { get; set; }

    public static AbpCacheData Create(object value, Type type, JsonSerializerOptions options)
    {
        if (value == null)
        {
            return new AbpCacheData
            {
                Type = type?.AssemblyQualifiedName,
                Payload = null
            };
        }

        var actualType = type ?? value.GetType();

        return new AbpCacheData
        {
            Type = actualType.AssemblyQualifiedName,
            Payload = JsonSerializer.Serialize(value, actualType, options)
        };
    }

    public static AbpCacheData? Deserialize(string serializedValue, JsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(serializedValue))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AbpCacheData>(serializedValue, options);
    }
}
