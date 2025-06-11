using DynamicModelBuilder.Services;
using FluentAssertions;
using Xunit;

namespace DynamicModelBuilder.Tests.Services;

public class SalaryCalculationServiceTests
{
    private readonly SalaryCalculationService _service;

    public SalaryCalculationServiceTests()
    {
        _service = new SalaryCalculationService();
    }

    [Fact]
    public void CalculateCompensationPackage_WithValidData_ShouldReturnAnalysis()
    {
        // Arrange
        var personData = new
        {
            FirstName = "John",
            Age = 35,
            Email = "john@company.com",
            Salary = 75000m,
            Department = "Engineering"
        };

        // Act
        var result = _service.CalculateCompensationPackage(personData);

        // Assert
        result.Should().NotBeNull();
        result.BaseSalary.Should().Be(75000m);
        result.DepartmentMultiplier.Should().Be(1.15m); // Engineering multiplier
        result.AnnualBonus.Should().BeGreaterThan(0);
        result.StockOptions.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateCompensationPackage_WithMissingOptionalProperties_ShouldUseDefaults()
    {
        // Arrange
        var incompleteData = new
        {
            FirstName = "Jane",
            Age = 25
            // Missing Salary, Department
        };

        // Act
        var result = _service.CalculateCompensationPackage(incompleteData);

        // Assert
        result.Should().NotBeNull();
        result.BaseSalary.Should().Be(50000m); // Default salary
        result.DepartmentMultiplier.Should().Be(1.0m); // Unknown department multiplier
    }

    [Fact]
    public void CalculateCompensationPackage_WithoutAge_ShouldThrowException()
    {
        // Arrange
        var dataWithoutAge = new
        {
            FirstName = "John",
            Salary = 75000m
            // Missing required Age property
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.CalculateCompensationPackage(dataWithoutAge));

        exception.Message.Should().Contain("Required property 'Age'");
    }

    [Theory]
    [InlineData("Engineering", 1.15)]
    [InlineData("Sales", 1.10)]
    [InlineData("Marketing", 1.05)]
    [InlineData("UnknownDept", 1.0)]
    public void CalculateCompensationPackage_DepartmentMultipliers_ShouldBeCorrect(string department, decimal expectedMultiplier)
    {
        // Arrange
        var personData = new
        {
            Age = 30,
            Salary = 50000m,
            Department = department
        };

        // Act
        var result = _service.CalculateCompensationPackage(personData);

        // Assert
        result.DepartmentMultiplier.Should().Be(expectedMultiplier);
    }
}