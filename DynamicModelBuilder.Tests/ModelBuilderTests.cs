using DynamicModelBuilder.Models;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace DynamicModelBuilder.Tests;

public class ModelBuilderTests
{
    [Fact]
    public void CreateModel_WithValidJson_ShouldCreateDynamicModel()
    {
        // Arrange
        var jsonDefinition = GetSamplePersonDefinition();

        // Act
        var model = ModelBuilder.CreateModel(jsonDefinition);

        // Assert
        model.Should().NotBeNull();
    }

    [Fact]
    public void DynamicModel_SetProperty_ShouldStoreValue()
    {
        // Arrange
        var model = ModelBuilder.CreateModel(GetSamplePersonDefinition());

        // Act
        model.SetProperty("FirstName", "John");
        model.SetProperty("Age", 30);

        // Assert
        model.GetProperty("FirstName").Should().Be("John");
        model.GetProperty("Age").Should().Be(30);
    }

    [Fact]
    public void DynamicModel_SetInvalidProperty_ShouldThrowException()
    {
        // Arrange
        var model = ModelBuilder.CreateModel(GetSamplePersonDefinition());

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            model.SetProperty("Age", 10)); // Below minimum age of 18

        exception.Message.Should().Contain("Validation failed for Age");
    }

    [Fact]
    public void SaveAsClass_ShouldGenerateFiles()
    {
        // Arrange
        var model = ModelBuilder.CreateModel(GetSamplePersonDefinition());
        var outputPath = Path.Combine(Path.GetTempPath(), "TestModels");

        // Act
        model.SaveAsClass("TestPerson", outputPath);

        // Assert
        var dtoFile = Path.Combine(Directory.GetCurrentDirectory(), outputPath, "TestPersonDto.cs");
        var domainFile = Path.Combine(Directory.GetCurrentDirectory(), outputPath, "TestPerson.cs");

        File.Exists(dtoFile).Should().BeTrue();
        File.Exists(domainFile).Should().BeTrue();

        // Cleanup
        if (Directory.Exists(outputPath))
            Directory.Delete(outputPath, true);
    }

    [Theory]
    [InlineData("John", 30, true)]
    [InlineData("", 30, false)]
    [InlineData("John", 10, false)]
    public void DynamicModel_Validation_ShouldWorkCorrectly(string firstName, int age, bool shouldPass)
    {
        // Arrange
        var model = ModelBuilder.CreateModel(GetSamplePersonDefinition());

        // Act & Assert
        if (shouldPass)
        {
            model.SetProperty("FirstName", firstName);
            model.SetProperty("Age", age);
            model.GetProperty("FirstName").Should().Be(firstName);
            model.GetProperty("Age").Should().Be(age);
        }
        else
        {
            Assert.ThrowsAny<ArgumentException>(() =>
            {
                model.SetProperty("FirstName", firstName);
                model.SetProperty("Age", age);
            });
        }
    }

    private string GetSamplePersonDefinition() => @"{
            ""OutputPath"": ""Models"",
            ""ClassName"": ""Person"",
            ""Specifications"": [
                {
                    ""Name"": ""IsValidAge"",
                    ""Expression"": ""Age >= 18 && Age <= 65"",
                    ""ErrorMessage"": ""Person must be of working age""
                },
                {
                    ""Name"": ""HasValidEmail"",
                    ""Expression"": ""Email.Contains('@') && Email.EndsWith('.com')"",
                    ""ErrorMessage"": ""Must be a valid .com email""
                }
            ],
            ""DomainRules"": [
                {
                    ""Name"": ""CanRetire"",
                    ""Expression"": ""Age >= 65"",
                    ""ReturnType"": ""bool""
                },
                {
                    ""Name"": ""GetDisplayName"",
                    ""Expression"": ""FirstName + ' (' + Age + ' years old)'"",
                    ""ReturnType"": ""string""
                }
            ],
            ""Properties"": [
                {
                    ""Name"": ""FirstName"",
                    ""Type"": ""string"",
                    ""Rules"": [
                        { ""RuleType"": ""required"", ""ErrorMessage"": ""First name is required"" },
                        { ""RuleType"": ""minlength"", ""Value"": 2, ""ErrorMessage"": ""First name must be at least 2 characters"" },
                        { ""RuleType"": ""maxlength"", ""Value"": 50 }
                    ],
                    ""Specifications"": [""HasValidName""]
                },
                {
                    ""Name"": ""Age"",
                    ""Type"": ""int"",
                    ""Rules"": [
                        { ""RuleType"": ""range"", ""Value"": { ""min"": 18, ""max"": 100 }, ""ErrorMessage"": ""Age must be between 18 and 100"" }
                    ],
                    ""Specifications"": [""IsValidAge""]
                },
                {
                    ""Name"": ""Email"",
                    ""Type"": ""string"",
                    ""Rules"": [
                        { ""RuleType"": ""required"", ""ErrorMessage"": ""Email is required"" },
                        { ""RuleType"": ""email"", ""ErrorMessage"": ""Invalid email format"" }
                    ],
                    ""Specifications"": [""HasValidEmail""]
                },
                {
                    ""Name"": ""Salary"",
                    ""Type"": ""decimal"",
                    ""Rules"": [
                        { ""RuleType"": ""range"", ""Value"": { ""min"": 0, ""max"": 1000000 } }
                    ]
                },
                {
                    ""Name"": ""Department"",
                    ""Type"": ""string"",
                    ""Rules"": [
                        { ""RuleType"": ""required"" }
                    ]
                }
            ]
        }";
}