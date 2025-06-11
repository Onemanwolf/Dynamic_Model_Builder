using DynamicModelBuilder.Services;
using FluentAssertions;
using Xunit;

namespace DynamicModelBuilder.Tests.Services;

public class BaseServiceTests
{
    private readonly TestBaseService _service;

    public BaseServiceTests()
    {
        _service = new TestBaseService();
    }

    [Fact]
    public void GetOptionalProperty_WithExistingProperty_ShouldReturnValue()
    {
        // Arrange
        var testObject = new { Name = "John", Age = 30 };

        // Act
        var name = _service.TestGetOptionalProperty<string>(testObject, "Name", "Default");
        var age = _service.TestGetOptionalProperty<int>(testObject, "Age", 0);

        // Assert
        name.Should().Be("John");
        age.Should().Be(30);
    }

    [Fact]
    public void GetOptionalProperty_WithMissingProperty_ShouldReturnDefault()
    {
        // Arrange
        var testObject = new { Name = "John" };

        // Act
        var missingValue = _service.TestGetOptionalProperty<string>(testObject, "MissingProp", "DefaultValue");

        // Assert
        missingValue.Should().Be("DefaultValue");
    }

    [Fact]
    public void GetRequiredProperty_WithExistingProperty_ShouldReturnValue()
    {
        // Arrange
        var testObject = new { Name = "John", Age = 30 };

        // Act
        var name = _service.TestGetRequiredProperty<string>(testObject, "Name");

        // Assert
        name.Should().Be("John");
    }

    [Fact]
    public void GetRequiredProperty_WithMissingProperty_ShouldThrowException()
    {
        // Arrange
        var testObject = new { Name = "John" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.TestGetRequiredProperty<string>(testObject, "MissingProp"));

        exception.Message.Should().Contain("Required property 'MissingProp'");
    }

    [Fact]
    public void GetOptionalProperty_WithTypeConversion_ShouldConvert()
    {
        // Arrange
        var testObject = new { StringNumber = "42" };

        // Act
        var convertedValue = _service.TestGetOptionalProperty<int>(testObject, "StringNumber", 0);

        // Assert
        convertedValue.Should().Be(42);
    }
}

// Test helper class to expose protected methods
public class TestBaseService : BaseService
{
    public T TestGetOptionalProperty<T>(object obj, string propertyName, T defaultValue = default(T))
        => GetOptionalProperty<T>(obj, propertyName, defaultValue);

    public T TestGetRequiredProperty<T>(object obj, string propertyName)
        => GetRequiredProperty<T>(obj, propertyName);
}