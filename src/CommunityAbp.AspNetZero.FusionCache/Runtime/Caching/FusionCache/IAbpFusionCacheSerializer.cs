namespace CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;

/// <summary>
/// Defines methods for serializing and deserializing objects for use with FusionCache.
/// </summary>
/// <remarks>Implementations of this interface are responsible for converting objects to and from a
/// serialized string representation suitable for caching. The serializer should ensure that the output is
/// compatible with the cache's storage and retrieval mechanisms. Thread safety and performance characteristics may
/// vary depending on the implementation.</remarks>
public interface IAbpFusionCacheSerializer
{
    /// <summary>
    /// Serializes the specified object to a string representation.
    /// </summary>
    /// <param name="value">The object to serialize. Cannot be null.</param>
    /// <param name="type">The type to use for serialization. If null, the runtime type of <paramref name="value"/> is used.</param>
    /// <returns>A string that represents the serialized form of the object.</returns>
    string? Serialize(object value, Type type = null);

    /// <summary>
    /// Deserializes the specified string into an object of the given type.
    /// </summary>
    /// <param name="serializedValue">The string containing the serialized representation of the object to deserialize. Cannot be null.</param>
    /// <param name="type">The type of the object to deserialize to. If null, a default or inferred type may be used, depending on the
    /// implementation.</param>
    /// <returns>An object that represents the deserialized data. The returned object's type corresponds to the specified
    /// type parameter, or a default type if none is provided.</returns>
    object? Deserialize(string serializedValue, Type type = null);

    /// <summary>
    /// Deserializes the specified string into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="serializedValue">The string containing the serialized representation of the object. Cannot be null or empty.</param>
    /// <returns>An instance of type T that represents the deserialized object.</returns>
    T? Deserialize<T>(string serializedValue);
}
