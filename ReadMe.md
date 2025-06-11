# Dynamic Model Builder

A powerful, JSON-driven model generation tool that enables rapid development of domain models with built-in validation, business logic, and specification patterns. Perfect for customer onboarding, prototyping, and configuration-driven applications.

## 🚀 Features

- **JSON-Driven Model Generation**: Define models, validation rules, and business logic through JSON configuration
- **Specification Pattern Support**: Implement reusable business rules and domain-specific logic
- **Dual Model Generation**: Creates both DTO (with validation attributes) and Domain models (with business methods)
- **Reflection-Based Services**: Resilient business services that adapt to model changes without breaking
- **Type Safety with Flexibility**: Automatic type conversion and graceful property handling
- **Extensible Architecture**: Easy to add new validation rules, specifications, and business services

## 📋 Table of Contents

- Quick Start
- Architecture Overview
- JSON Configuration
- Generated Models
- Business Services
- Extensibility
- Examples
- Best Practices
- Contributing

## 🎯 Quick Start

### 1. Define Your Model in JSON

```json
{
  "OutputPath": "Models",
  "ClassName": "Person",
  "Specifications": [
    {
      "Name": "IsValidAge",
      "Expression": "Age >= 18 && Age <= 65",
      "ErrorMessage": "Person must be of working age"
    },
    {
      "Name": "HasValidEmail",
      "Expression": "Email.Contains('@') && Email.EndsWith('.com')",
      "ErrorMessage": "Must be a valid .com email"
    }
  ],
  "DomainRules": [
    {
      "Name": "CanRetire",
      "Expression": "Age >= 65",
      "ReturnType": "bool"
    },
    {
      "Name": "GetDisplayName",
      "Expression": "FirstName + ' (' + Age + ' years old)'",
      "ReturnType": "string"
    }
  ],
  "Properties": [
    {
      "Name": "FirstName",
      "Type": "string",
      "Rules": [
        { "RuleType": "required", "ErrorMessage": "First name is required" },
        { "RuleType": "minlength", "Value": 2 },
        { "RuleType": "maxlength", "Value": 50 }
      ]
    },
    {
      "Name": "Age",
      "Type": "int",
      "Rules": [
        { "RuleType": "range", "Value": { "min": 18, "max": 100 } }
      ]
    },
    {
      "Name": "Email",
      "Type": "string",
      "Rules": [
        { "RuleType": "required" },
        { "RuleType": "email" }
      ]
    }
  ]
}
```

### 2. Generate Models

```csharp
var model = ModelBuilder.CreateModel(jsonDefinition);
model.SaveAsClass("Person", "Models");
```

### 3. Use Generated Models

```csharp
// DTO with validation attributes
var personDto = new PersonDto
{
    FirstName = "John",
    Age = 30,
    Email = "john@company.com"
};

// Domain model with business logic
var person = new Person
{
    FirstName = "John",
    Age = 30,
    Email = "john@company.com"
};

// Use specifications and domain rules
bool isValidAge = person.IsValidAge();
bool canRetire = person.CanRetire();
string displayName = person.GetDisplayName();
```

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   JSON Config   │───▶│  Model Builder   │───▶│  Generated Models   │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
                                                           │
                                                           ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│ Business Logic  │◀───│  Base Service    │◀───│   Domain Services   │
│   Services      │    │  (Reflection)    │    │   (Extensible)      │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
```

### Core Components

1. **ModelBuilder**: Parses JSON and generates C# classes
2. **BaseService**: Provides reflection-based property access for services
3. **Generated Models**: DTO and Domain classes with validation and business logic
4. **Business Services**: Complex domain logic that operates on any model structure

## 📝 JSON Configuration

### Property Definition

```json
{
  "Name": "PropertyName",
  "Type": "string|int|decimal|bool",
  "Rules": [
    { "RuleType": "required", "ErrorMessage": "Custom error message" },
    { "RuleType": "minlength", "Value": 2 },
    { "RuleType": "maxlength", "Value": 50 },
    { "RuleType": "range", "Value": { "min": 0, "max": 100 } },
    { "RuleType": "email" }
  ],
  "Specifications": ["SpecificationName"]
}
```

### Specifications (Business Rules)

```json
{
  "Name": "IsValidAge",
  "Expression": "Age >= 18 && Age <= 65",
  "ErrorMessage": "Person must be of working age"
}
```

### Domain Rules (Computed Methods)

```json
{
  "Name": "CalculateBonus",
  "Expression": "Salary * 0.1",
  "ReturnType": "decimal"
}
```

### Supported Rule Types

| Rule Type | Description | Example |
|-----------|-------------|---------|
| `required` | Property must have a value | `{ "RuleType": "required" }` |
| `minlength` | Minimum string length | `{ "RuleType": "minlength", "Value": 2 }` |
| `maxlength` | Maximum string length | `{ "RuleType": "maxlength", "Value": 50 }` |
| `range` | Numeric range validation | `{ "RuleType": "range", "Value": { "min": 0, "max": 100 } }` |
| `email` | Email format validation | `{ "RuleType": "email" }` |

## 🏭 Generated Models

### DTO Model (with Validation Attributes)

```csharp
public class PersonDto
{
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2)]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Range(18, 100)]
    public int Age { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

### Domain Model (with Business Logic)

```csharp
public class Person
{
    public string FirstName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }

    /// <summary>
    /// Person must be of working age
    /// </summary>
    public bool IsValidAge()
    {
        return Age >= 18 && Age <= 65;
    }

    /// <summary>
    /// Must be a valid .com email
    /// </summary>
    public bool HasValidEmail()
    {
        return Email.Contains('@') && Email.EndsWith(".com");
    }

    /// <summary>
    /// Domain rule: CanRetire
    /// </summary>
    public bool CanRetire()
    {
        return Age >= 65;
    }
}
```

## 🔧 Business Services

### Creating Resilient Services

```csharp
public class SalaryCalculationService : BaseService
{
    public SalaryAnalysis CalculateCompensationPackage(object person)
    {
        // Safe property access with fallbacks
        var baseSalary = GetOptionalProperty<decimal>(person, "Salary", 50000m);
        var department = GetOptionalProperty<string>(person, "Department", "Unknown");
        var age = GetRequiredProperty<int>(person, "Age"); // Required for calculations

        // Complex business logic here...
        return new SalaryAnalysis
        {
            BaseSalary = baseSalary,
            AnnualBonus = CalculateBonus(baseSalary, department),
            // ...
        };
    }
}
```

### BaseService Methods

| Method | Description | Usage |
|--------|-------------|--------|
| `GetRequiredProperty<T>()` | Gets property value, throws if missing | Critical business logic |
| `GetOptionalProperty<T>()` | Gets property with fallback value | Optional calculations |
| `ValidateRequiredProperties()` | Validates multiple properties exist | Service initialization |

## 🔄 Extensibility

### Adding New Validation Rules

```csharp
// In RuleDefinition processing
case "regex":
    if (rule.Value is string pattern && value is string strValue)
    {
        if (!Regex.IsMatch(strValue, pattern))
            errors.Add(rule.ErrorMessage ?? $"{name} doesn't match required pattern");
    }
    break;
```

### Creating Custom Services

```csharp
public class ComplianceService : BaseService
{
    public ComplianceResult ValidateCompliance(object employee)
    {
        // Automatically works with any generated model
        var department = GetOptionalProperty<string>(employee, "Department", "Unknown");
        var clearanceLevel = GetOptionalProperty<string>(employee, "ClearanceLevel", "Public");

        // Business logic...
    }
}
```

### Supporting New Expression Types

```csharp
private string CleanExpression(string expression)
{
    return expression
        .Replace(" MATCHES ", ".Match(")
        .Replace(" BETWEEN ", " >= ")
        // Add new operators...
}
```

## 📚 Examples

### Customer Onboarding Scenarios

#### Healthcare Customer
```json
{
  "ClassName": "Patient",
  "Properties": [
    {
      "Name": "MedicalId",
      "Type": "string",
      "Rules": [
        { "RuleType": "required" },
        { "RuleType": "regex", "Value": "^MED[0-9]{6}$" }
      ]
    }
  ],
  "DomainRules": [
    {
      "Name": "IsEligibleForTreatment",
      "Expression": "Age >= 18 && HasInsurance == true",
      "ReturnType": "bool"
    }
  ]
}
```

#### E-commerce Customer
```json
{
  "ClassName": "Product",
  "Properties": [
    {
      "Name": "SKU",
      "Type": "string",
      "Rules": [
        { "RuleType": "required" },
        { "RuleType": "regex", "Value": "^[A-Z]{3}-[0-9]{4}$" }
      ]
    }
  ],
  "DomainRules": [
    {
      "Name": "IsDiscountEligible",
      "Expression": "Price > 100 && Category == 'Electronics'",
      "ReturnType": "bool"
    }
  ]
}
```

### Dynamic Model Testing

```csharp
// Test with different model structures
var personData = new { FirstName = "John", Age = 30, Department = "IT" };
var contractorData = new { Name = "Jane", Years = 5, Company = "ABC Corp" };

// Same service works with both
var salaryService = new SalaryCalculationService();
var result1 = salaryService.CalculateCompensationPackage(personData);
var result2 = salaryService.CalculateCompensationPackage(contractorData);
```

## 🎯 Best Practices

### JSON Configuration
- **Keep expressions simple**: Complex logic should be in services, not JSON
- **Use meaningful names**: Clear property and specification names
- **Provide error messages**: Custom validation messages improve UX
- **Group related rules**: Use specifications for reusable business rules

### Service Development
- **Inherit from BaseService**: Use reflection-based property access
- **Handle missing properties**: Always provide sensible defaults
- **Validate critical properties**: Use `GetRequiredProperty` for essential data
- **Document assumptions**: Clear comments about expected properties

### Model Evolution
- **Backward compatibility**: New properties should be optional
- **Gradual migration**: Support multiple model versions simultaneously
- **Property mapping**: Use aliases for renamed properties
- **Version awareness**: Consider model versioning for breaking changes

## 🔍 Fault Tolerance Analysis

### ✅ **Resilient to Changes**

| Change Type | Impact | Service Response |
|-------------|---------|------------------|
| Add Property | ✅ None | Continues normally |
| Remove Optional Property | ✅ Uses default | Graceful degradation |
| Rename Property | ⚠️ Uses default | Logs warning |
| Change Property Type | ⚠️ Attempts conversion | May use default if conversion fails |

### ⚠️ **Potential Breaking Changes**

| Change Type | Impact | Mitigation |
|-------------|---------|------------|
| Remove Required Property | ❌ Service fails | Use `ValidateRequiredProperties` |
| Incompatible Type Change | ❌ Conversion fails | Version-aware processing |
| Business Logic Dependency | ❌ Logic invalid | Specification validation |

## 🚀 Use Cases

### Perfect For:
- **Customer Onboarding**: Rapid model creation for new customers
- **Prototyping**: Quick domain model generation
- **Configuration-Driven Apps**: Business users can modify models
- **A/B Testing**: Different model versions for testing
- **Multi-Tenant Applications**: Tenant-specific models

### Consider Alternatives For:
- **Mission-Critical Algorithms**: Use traditional code
- **Performance-Sensitive Operations**: Direct object access
- **Complex Relationships**: Entity Framework or similar
- **Advanced Type Systems**: Consider code-first approaches

## 🏆 Benefits Summary

1. **🚀 Rapid Development**: Generate models in minutes, not hours
2. **🔧 Maintainable**: JSON changes don't break services
3. **🔄 Flexible**: Supports evolving business requirements
4. **🛡️ Resilient**: Services adapt to model changes gracefully
5. **📈 Scalable**: Easy to add new customers and models
6. **🧪 Testable**: Services work with any object structure
7. **📚 Reusable**: Specifications and services are composable

## 📈 Getting Started

1. **Clone the repository**
2. **Define your first model** in JSON
3. **Run the ModelBuilder** to generate classes
4. **Create services** inheriting from BaseService
5. **Test with different model structures**
6. **Extend with custom rules and specifications**

This Dynamic Model Builder provides a powerful foundation for building flexible, maintainable applications that can evolve with changing business requirements while maintaining robust, fault-tolerant operation.

