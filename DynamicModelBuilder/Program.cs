using DynamicModelBuilder.Models;
using DynamicModelBuilder.Services;
using System.Text.Json;

namespace DynamicModelBuilder;

public class Program
{
    public static void Main()
    {
        string jsonDefinition = @"{
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
                },
                {
                    ""Name"": ""IsManager"",
                    ""Type"": ""bool"",
                    ""Rules"": []
                }
            ]
        }";

        try
        {
            var model = ModelBuilder.CreateModel(jsonDefinition);

            // Parse for output path and class name
            using var doc = JsonDocument.Parse(jsonDefinition);
            var root = doc.RootElement;
            var outputPath = root.GetProperty("OutputPath").GetString() ?? "Models";
            var className = root.GetProperty("ClassName").GetString() ?? "GeneratedClass";

            // Save the generated class files
            model.SaveAsClass(className, outputPath);
            Console.WriteLine($"Generated {className}.cs and {className}Dto.cs files in {outputPath} folder");

            // Test the dynamic model
            model.SetProperty("FirstName", "John");
            model.SetProperty("Age", 45);
            model.SetProperty("Email", "john.doe@company.com");
            model.SetProperty("Salary", 75000m);
            model.SetProperty("Department", "Engineering");

            Console.WriteLine("\n=== Dynamic Model Tests ===");
            Console.WriteLine($"FirstName: {model.GetProperty("FirstName")}");
            Console.WriteLine($"Age: {model.GetProperty("Age")}");
            Console.WriteLine($"Email: {model.GetProperty("Email")}");
            Console.WriteLine($"Salary: ${model.GetProperty("Salary"):N2}");
            Console.WriteLine($"Department: {model.GetProperty("Department")}");
            Console.WriteLine($"Valid Age: {model.EvaluateSpecification("IsValidAge")}");

            // Create test data object for services (this could be your generated DTO or any object)
            var personData = new
            {
                FirstName = "John",
                Age = 45,
                Email = "john.doe@company.com",
                Salary = 75000m,
                Department = "Engineering"
            };

            // Initialize complex business services
            var salaryCalculator = new SalaryCalculationService();
            var benefitsProcessor = new BenefitsProcessingService();
            var complianceValidator = new ComplianceValidationService();

            Console.WriteLine("\n=== Complex Business Logic Tests ===");

            // Test salary calculations with error handling
            try
            {
                var salaryAnalysis = salaryCalculator.CalculateCompensationPackage(personData);
                Console.WriteLine($"Annual Bonus: ${salaryAnalysis.AnnualBonus:N2}");
                Console.WriteLine($"Stock Options: {salaryAnalysis.StockOptions} shares");
                Console.WriteLine($"Total Compensation: ${salaryAnalysis.TotalCompensation:N2}");
                Console.WriteLine($"Department Multiplier: {salaryAnalysis.DepartmentMultiplier:P}");
                Console.WriteLine($"Risk Factor: {salaryAnalysis.RiskFactor:P}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Salary Calculation Error: {ex.Message}");
            }

            // Test benefits processing with error handling
            try
            {
                var benefitsPackage = benefitsProcessor.ProcessBenefitsEligibility(personData);
                Console.WriteLine($"Health Insurance Premium: ${benefitsPackage.HealthInsurancePremium:N2}");
                Console.WriteLine($"401k Match: ${benefitsPackage.RetirementMatch:N2}");
                Console.WriteLine($"Vacation Days: {benefitsPackage.VacationDays}");
                Console.WriteLine($"Life Insurance Coverage: ${benefitsPackage.LifeInsuranceCoverage:N2}");
                Console.WriteLine($"Flex Spending Account: ${benefitsPackage.FlexSpendingAccount:N2}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Benefits Processing Error: {ex.Message}");
            }

            // Test compliance validation with error handling
            try
            {
                var complianceResult = complianceValidator.ValidateEmployeeCompliance(personData);
                Console.WriteLine($"Background Check Required: {complianceResult.BackgroundCheckRequired}");
                Console.WriteLine($"Security Clearance Level: {complianceResult.SecurityClearanceLevel}");
                Console.WriteLine($"Compliance Score: {complianceResult.ComplianceScore}%");
                Console.WriteLine($"Compliance Status: {complianceResult.ComplianceStatus}");
                if (complianceResult.RegulatoryFlags.Any())
                {
                    Console.WriteLine("Regulatory Flags:");
                    foreach (var flag in complianceResult.RegulatoryFlags)
                    {
                        Console.WriteLine($"  - {flag}");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Compliance Validation Error: {ex.Message}");
            }

            // Test with object missing some properties to demonstrate resilience
            Console.WriteLine("\n=== Testing with Incomplete Data ===");
            var incompleteData = new
            {
                FirstName = "Jane",
                Age = 30
                // Missing Email, Salary, Department
            };

            try
            {
                var incompleteSalaryAnalysis = salaryCalculator.CalculateCompensationPackage(incompleteData);
                Console.WriteLine($"Incomplete Data - Total Compensation: ${incompleteSalaryAnalysis.TotalCompensation:N2}");
                Console.WriteLine("✓ Service handled missing properties with defaults");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected Error with Incomplete Data: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        // Console.WriteLine("\nPress any key to exit...");
        // Console.ReadKey();
    }
}