using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;
using System.Text.Json;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class AbpCacheDataTests
{
    private static JsonSerializerOptions TestOptions => new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };

    #region Create Tests

    [Test]
    public async Task Create_WithStringValue_ShouldSetTypeAndPayload()
    {
        // Arrange
        var value = "test string";
        var type = typeof(string);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Type).Contains("System.String");
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("test string");
    }

    [Test]
    public async Task Create_WithIntValue_ShouldSetTypeAndPayload()
    {
        // Arrange
        var value = 42;
        var type = typeof(int);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Type).Contains("System.Int32");
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("42");
    }

    [Test]
    public async Task Create_WithComplexObject_ShouldSerializeProperties()
    {
        // Arrange
        var value = new TestDataObject
        {
            Id = 1,
            Name = "Test",
            IsActive = true
        };
        var type = typeof(TestDataObject);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Type).Contains("TestDataObject");
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("\"id\":1");
        await Assert.That(result.Payload).Contains("Test");
    }

    [Test]
    public async Task Create_WithNullValue_ShouldSetNullPayload()
    {
        // Arrange
        object? value = null;
        var type = typeof(string);

        // Act
        var result = AbpCacheData.Create(value!, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Payload).IsNull();
    }

    [Test]
    public async Task Create_WithNullType_ShouldInferTypeFromValue()
    {
        // Arrange
        var value = "test string";
        Type? type = null;

        // Act
        var result = AbpCacheData.Create(value, type!, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Type).Contains("System.String");
    }

    [Test]
    public async Task Create_WithCollectionValue_ShouldSerializeAllItems()
    {
        // Arrange
        var value = new List<int> { 1, 2, 3, 4, 5 };
        var type = typeof(List<int>);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("[1,2,3,4,5]");
    }

    [Test]
    public async Task Create_WithDictionary_ShouldSerializeAllEntries()
    {
        // Arrange
        var value = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 }
        };
        var type = typeof(Dictionary<string, int>);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("one");
        await Assert.That(result.Payload).Contains("two");
    }

    #endregion

    #region Deserialize Tests

    [Test]
    public async Task Deserialize_WithValidJson_ShouldReturnAbpCacheData()
    {
        // Arrange
        var json = "{\"type\":\"System.String\",\"payload\":\"\\\"test\\\"\"}";

        // Act
        var result = AbpCacheData.Deserialize(json, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Type).IsEqualTo("System.String");
        await Assert.That(result.Payload).IsNotNull();
    }

    [Test]
    public async Task Deserialize_WithNullJson_ShouldReturnNull()
    {
        // Act
        var result = AbpCacheData.Deserialize(null!, TestOptions);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Deserialize_WithEmptyString_ShouldReturnNull()
    {
        // Act
        var result = AbpCacheData.Deserialize(string.Empty, TestOptions);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Deserialize_WithComplexObjectJson_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = "{\"type\":\"TestDataObject\",\"payload\":\"{\\\"id\\\":1,\\\"name\\\":\\\"Test\\\",\\\"isActive\\\":true}\"}";

        // Act
        var result = AbpCacheData.Deserialize(json, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Type).IsEqualTo("TestDataObject");
        await Assert.That(result.Payload).IsNotNull();
    }

    #endregion

    #region Round-trip Tests

    [Test]
    public async Task RoundTrip_WithString_ShouldPreserveValueAndType()
    {
        // Arrange
        var originalValue = "test string";
        var originalType = typeof(string);

        // Act
        var cacheData = AbpCacheData.Create(originalValue, originalType, TestOptions);
        var json = System.Text.Json.JsonSerializer.Serialize(cacheData, TestOptions);
        var deserialized = AbpCacheData.Deserialize(json, TestOptions);

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Type).Contains("System.String");
        await Assert.That(deserialized.Payload).Contains("test string");
    }

    [Test]
    public async Task RoundTrip_WithInt_ShouldPreserveValueAndType()
    {
        // Arrange
        var originalValue = 42;
        var originalType = typeof(int);

        // Act
        var cacheData = AbpCacheData.Create(originalValue, originalType, TestOptions);
        var json = System.Text.Json.JsonSerializer.Serialize(cacheData, TestOptions);
        var deserialized = AbpCacheData.Deserialize(json, TestOptions);

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Type).Contains("System.Int32");
        await Assert.That(deserialized.Payload).Contains("42");
    }

    [Test]
    public async Task RoundTrip_WithList_ShouldPreserveValueAndType()
    {
        // Arrange
        var originalValue = new List<string> { "one", "two", "three" };
        var originalType = typeof(List<string>);

        // Act
        var cacheData = AbpCacheData.Create(originalValue, originalType, TestOptions);
        var json = System.Text.Json.JsonSerializer.Serialize(cacheData, TestOptions);
        var deserialized = AbpCacheData.Deserialize(json, TestOptions);

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Type).Contains("List");
        await Assert.That(deserialized.Payload).Contains("one");
        await Assert.That(deserialized.Payload).Contains("two");
        await Assert.That(deserialized.Payload).Contains("three");
    }

    #endregion

    #region Type Preservation Tests

    [Test]
    public async Task Create_ShouldIncludeAssemblyQualifiedName()
    {
        // Arrange
        var value = "test";
        var type = typeof(string);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Type).Contains("System.String");
        // AssemblyQualifiedName includes assembly info
        await Assert.That(result.Type).Contains(",");
    }

    [Test]
    public async Task Create_WithGenericType_ShouldPreserveGenericTypeInfo()
    {
        // Arrange
        var value = new List<int> { 1, 2, 3 };
        var type = typeof(List<int>);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Type).Contains("List");
        await Assert.That(result.Type).Contains("Int32");
    }

    [Test]
    public async Task Create_WithNestedGenericType_ShouldPreserveNestedTypeInfo()
    {
        // Arrange
        var value = new Dictionary<string, List<int>>
        {
            { "numbers", new List<int> { 1, 2, 3 } }
        };
        var type = typeof(Dictionary<string, List<int>>);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result.Type).IsNotNull();
        await Assert.That(result.Type).Contains("Dictionary");
        await Assert.That(result.Type).Contains("String");
        await Assert.That(result.Type).Contains("List");
    }

    #endregion

    #region Null and Edge Cases

    [Test]
    public async Task Create_WithNullValueAndNullType_ShouldHandleGracefully()
    {
        // Arrange
        object? value = null;
        Type? type = null;

        // Act
        var result = AbpCacheData.Create(value!, type!, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Payload).IsNull();
    }

    [Test]
    public async Task Create_WithEmptyString_ShouldPreserveEmptyString()
    {
        // Arrange
        var value = string.Empty;
        var type = typeof(string);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("\"\"");
    }

    [Test]
    public async Task Create_WithZeroValue_ShouldPreserveZero()
    {
        // Arrange
        var value = 0;
        var type = typeof(int);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("0");
    }

    [Test]
    public async Task Create_WithFalseBoolean_ShouldPreserveFalse()
    {
        // Arrange
        var value = false;
        var type = typeof(bool);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Payload).IsNotNull();
        await Assert.That(result.Payload).Contains("false");
    }

    #endregion

    #region CamelCase Serialization Tests

    [Test]
    public async Task Create_ShouldUseCamelCaseForPropertyNames()
    {
        // Arrange
        var value = "test";
        var type = typeof(string);

        // Act
        var result = AbpCacheData.Create(value, type, TestOptions);
        var json = System.Text.Json.JsonSerializer.Serialize(result, TestOptions);

        // Assert
        // Properties should be camelCase in JSON
        await Assert.That(json).Contains("\"type\":");
        await Assert.That(json).Contains("\"payload\":");
        // Should NOT contain PascalCase
        await Assert.That(json).DoesNotContain("\"Type\":");
        await Assert.That(json).DoesNotContain("\"Payload\":");
    }

    [Test]
    public async Task Deserialize_ShouldHandleCamelCasePropertyNames()
    {
        // Arrange
        var json = "{\"type\":\"System.String\",\"payload\":\"\\\"test\\\"\"}";

        // Act
        var result = AbpCacheData.Deserialize(json, TestOptions);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Type).IsEqualTo("System.String");
    }

    #endregion

    #region Test Helper Classes

    public class TestDataObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    #endregion
}
