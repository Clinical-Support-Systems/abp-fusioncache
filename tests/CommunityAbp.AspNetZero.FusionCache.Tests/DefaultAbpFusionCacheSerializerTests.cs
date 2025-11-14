using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class DefaultAbpFusionCacheSerializerTests
{
    private DefaultAbpFusionCacheSerializer _serializer = null!;

    [Before(Test)]
    public void Setup()
    {
        _serializer = new DefaultAbpFusionCacheSerializer();
    }

    #region Serialize Tests

    [Test]
    public async Task Serialize_WithNullValue_ShouldReturnNull()
    {
        // Act
        var result = _serializer.Serialize(null!);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Serialize_WithStringValue_ShouldReturnSerializedJson()
    {
        // Arrange
        var value = "test string";

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("test string");
        await Assert.That(result).Contains("\"type\":");
        await Assert.That(result).Contains("\"payload\":");
    }

    [Test]
    public async Task Serialize_WithIntValue_ShouldReturnSerializedJson()
    {
        // Arrange
        var value = 42;

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("42");
    }

    [Test]
    public async Task Serialize_WithComplexObject_ShouldSerializeAllProperties()
    {
        // Arrange
        var value = new TestComplexObject
        {
            Id = 1,
            Name = "Test",
            CreatedDate = new DateTime(2024, 1, 1),
            IsActive = true
        };

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("\"id\":1");
        await Assert.That(result).Contains("Test");
        await Assert.That(result).Contains("isActive");
    }

    [Test]
    public async Task Serialize_WithCollection_ShouldSerializeAllItems()
    {
        // Arrange
        var value = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("[1,2,3,4,5]");
    }

    [Test]
    public async Task Serialize_WithExplicitType_ShouldUseProvidedType()
    {
        // Arrange
        object value = "test";
        var explicitType = typeof(string);

        // Act
        var result = _serializer.Serialize(value, explicitType);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("System.String");
    }

    #endregion

    #region Deserialize Tests

    [Test]
    public async Task Deserialize_WithNullValue_ShouldReturnNull()
    {
        // Act
        var result = _serializer.Deserialize(null!);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Deserialize_WithEmptyString_ShouldReturnNull()
    {
        // Act
        var result = _serializer.Deserialize(string.Empty);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Deserialize_WithValidSerializedString_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalValue = "test string";
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(originalValue);
    }

    [Test]
    public async Task Deserialize_WithValidSerializedInt_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalValue = 42;
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(originalValue);
    }

    [Test]
    public async Task Deserialize_WithComplexObject_ShouldReturnOriginalObject()
    {
        // Arrange
        var originalValue = new TestComplexObject
        {
            Id = 1,
            Name = "Test",
            CreatedDate = new DateTime(2024, 1, 1),
            IsActive = true
        };
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize(serialized!) as TestComplexObject;

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(originalValue.Id);
        await Assert.That(result.Name).IsEqualTo(originalValue.Name);
        await Assert.That(result.CreatedDate).IsEqualTo(originalValue.CreatedDate);
        await Assert.That(result.IsActive).IsEqualTo(originalValue.IsActive);
    }

    [Test]
    public async Task Deserialize_WithCollection_ShouldReturnOriginalCollection()
    {
        // Arrange
        var originalValue = new List<int> { 1, 2, 3, 4, 5 };
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize(serialized!) as List<int>;

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Count).IsEqualTo(5);
        await Assert.That(result).IsEquivalentTo(originalValue);
    }

    [Test]
    public async Task Deserialize_WithExplicitType_ShouldUseProvidedType()
    {
        // Arrange
        var originalValue = "test string";
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize(serialized!, typeof(string));

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(originalValue);
        await Assert.That(result).IsTypeOf(typeof(string));
    }

    [Test]
    public async Task Deserialize_WithInvalidJson_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act & Assert
        await Assert.That(() => Task.FromResult(_serializer.Deserialize(invalidJson)))
            .Throws<InvalidOperationException>()
            .With(ex => ex.Message.Contains("Failed to deserialize cache value"));
    }

    #endregion

    #region Generic Deserialize Tests

    [Test]
    public async Task DeserializeGeneric_WithValidSerializedString_ShouldReturnTypedValue()
    {
        // Arrange
        var originalValue = "test string";
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize<string>(serialized!);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(originalValue);
    }

    [Test]
    public async Task DeserializeGeneric_WithValidSerializedInt_ShouldReturnTypedValue()
    {
        // Arrange
        var originalValue = 42;
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize<int>(serialized!);

        // Assert
        await Assert.That(result).IsEqualTo(originalValue);
    }

    [Test]
    public async Task DeserializeGeneric_WithComplexObject_ShouldReturnTypedObject()
    {
        // Arrange
        var originalValue = new TestComplexObject
        {
            Id = 1,
            Name = "Test",
            CreatedDate = new DateTime(2024, 1, 1),
            IsActive = true
        };
        var serialized = _serializer.Serialize(originalValue);

        // Act
        var result = _serializer.Deserialize<TestComplexObject>(serialized!);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(originalValue.Id);
        await Assert.That(result.Name).IsEqualTo(originalValue.Name);
        await Assert.That(result.CreatedDate).IsEqualTo(originalValue.CreatedDate);
        await Assert.That(result.IsActive).IsEqualTo(originalValue.IsActive);
    }

    [Test]
    public async Task DeserializeGeneric_WithNull_ShouldReturnDefault()
    {
        // Act
        var result = _serializer.Deserialize<string>(null!);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DeserializeGeneric_WithNullForValueType_ShouldReturnDefault()
    {
        // Act
        var result = _serializer.Deserialize<int>(null!);

        // Assert
        await Assert.That(result).IsEqualTo(default(int));
    }

    #endregion

    #region Round-trip Tests

    [Test]
    public async Task RoundTrip_WithString_ShouldPreserveValue()
    {
        // Arrange
        var originalValue = "Hello, World!";

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(originalValue);
    }

    [Test]
    public async Task RoundTrip_WithInt_ShouldPreserveValue()
    {
        // Arrange
        var originalValue = 12345;

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(originalValue);
    }

    [Test]
    public async Task RoundTrip_WithDouble_ShouldPreserveValue()
    {
        // Arrange
        var originalValue = 123.45;

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(originalValue);
    }

    [Test]
    public async Task RoundTrip_WithBoolean_ShouldPreserveValue()
    {
        // Arrange
        var originalValue = true;

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(originalValue);
    }

    [Test]
    public async Task RoundTrip_WithDateTime_ShouldPreserveValue()
    {
        // Arrange
        var originalValue = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(originalValue);
    }

    [Test]
    public async Task RoundTrip_WithComplexObject_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalValue = new TestComplexObject
        {
            Id = 100,
            Name = "Complex Test",
            CreatedDate = new DateTime(2024, 6, 15),
            IsActive = false
        };

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!) as TestComplexObject;

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Id).IsEqualTo(originalValue.Id);
        await Assert.That(deserialized.Name).IsEqualTo(originalValue.Name);
        await Assert.That(deserialized.CreatedDate).IsEqualTo(originalValue.CreatedDate);
        await Assert.That(deserialized.IsActive).IsEqualTo(originalValue.IsActive);
    }

    [Test]
    public async Task RoundTrip_WithNestedObject_ShouldPreserveStructure()
    {
        // Arrange
        var originalValue = new TestNestedObject
        {
            Id = 1,
            Child = new TestComplexObject
            {
                Id = 2,
                Name = "Nested",
                CreatedDate = new DateTime(2024, 1, 1),
                IsActive = true
            }
        };

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!) as TestNestedObject;

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Id).IsEqualTo(originalValue.Id);
        await Assert.That(deserialized.Child).IsNotNull();
        await Assert.That(deserialized.Child!.Id).IsEqualTo(originalValue.Child.Id);
        await Assert.That(deserialized.Child.Name).IsEqualTo(originalValue.Child.Name);
    }

    [Test]
    public async Task RoundTrip_WithList_ShouldPreserveAllItems()
    {
        // Arrange
        var originalValue = new List<string> { "one", "two", "three" };

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!) as List<string>;

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Count).IsEqualTo(3);
        await Assert.That(deserialized).IsEquivalentTo(originalValue);
    }

    [Test]
    public async Task RoundTrip_WithDictionary_ShouldPreserveAllEntries()
    {
        // Arrange
        var originalValue = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        // Act
        var serialized = _serializer.Serialize(originalValue);
        var deserialized = _serializer.Deserialize(serialized!) as Dictionary<string, int>;

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Count).IsEqualTo(3);
        await Assert.That(deserialized["one"]).IsEqualTo(1);
        await Assert.That(deserialized["two"]).IsEqualTo(2);
        await Assert.That(deserialized["three"]).IsEqualTo(3);
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task Serialize_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var value = "Special: \n\t\r\"'<>&";

        // Act
        var serialized = _serializer.Serialize(value);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(value);
    }

    [Test]
    public async Task Serialize_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var value = "Unicode: 你好 مرحبا 🎉";

        // Act
        var serialized = _serializer.Serialize(value);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(value);
    }

    [Test]
    public async Task Serialize_WithEmptyString_ShouldHandleCorrectly()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var serialized = _serializer.Serialize(value);
        var deserialized = _serializer.Deserialize(serialized!);

        // Assert
        await Assert.That(deserialized).IsEqualTo(value);
    }

    [Test]
    public async Task Serialize_WithEmptyCollection_ShouldHandleCorrectly()
    {
        // Arrange
        var value = new List<int>();

        // Act
        var serialized = _serializer.Serialize(value);
        var deserialized = _serializer.Deserialize(serialized!) as List<int>;

        // Assert
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Count).IsEqualTo(0);
    }

    #endregion

    #region Test Helper Classes

    public class TestComplexObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class TestNestedObject
    {
        public int Id { get; set; }
        public TestComplexObject? Child { get; set; }
    }

    #endregion
}
