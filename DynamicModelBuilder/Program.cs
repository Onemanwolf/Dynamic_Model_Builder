using DynamicModelBuilder.Models;
using System.Text.Json;

namespace DynamicModelBuilder;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Dynamic Model Builder - Demo");
        Console.WriteLine("============================");

        // Simple demonstration of model generation
        string jsonDefinition = GetSamplePersonDefinition();

        try
        {
            var model = ModelBuilder.CreateModel(jsonDefinition);

            using var doc = JsonDocument.Parse(jsonDefinition);
            var root = doc.RootElement;
            var outputPath = root.GetProperty("OutputPath").GetString() ?? "Models";
            var className = root.GetProperty("ClassName").GetString() ?? "GeneratedClass";

            model.SaveAsClass(className, outputPath);
            Console.WriteLine($"✅ Generated {className}.cs and {className}Dto.cs in {outputPath} folder");

            Console.WriteLine("\n📋 To run comprehensive tests:");
            Console.WriteLine("   dotnet test");

            Console.WriteLine("\n📖 For more examples, see the unit tests in the test project.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static string GetSamplePersonDefinition()
    {
        return @"{
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
}