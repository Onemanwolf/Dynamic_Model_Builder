using DynamicModelBuilder.Models;

namespace DynamicModelBuilder.Services;

public class SalaryCalculationService : BaseService
{
    private readonly Dictionary<string, decimal> _departmentMultipliers = new()
    {
        ["Engineering"] = 1.15m,
        ["Sales"] = 1.10m,
        ["Marketing"] = 1.05m,
        ["HR"] = 1.00m,
        ["Finance"] = 1.08m
    };

    private readonly Dictionary<int, decimal> _experienceMultipliers = new()
    {
        [1] = 1.0m,   // 18-25
        [2] = 1.2m,   // 26-35
        [3] = 1.4m,   // 36-45
        [4] = 1.6m,   // 46-55
        [5] = 1.8m    // 56+
    };

    public SalaryAnalysis CalculateCompensationPackage(object person)
    {
        // Use reflection to safely access properties with fallbacks
        var baseSalary = GetOptionalProperty<decimal>(person, "Salary", 50000m);
        var department = GetOptionalProperty<string>(person, "Department", "Unknown");
        var age = GetRequiredProperty<int>(person, "Age");
        var email = GetOptionalProperty<string>(person, "Email", "");

        var departmentMultiplier = _departmentMultipliers.GetValueOrDefault(department, 1.0m);
        var experienceLevel = GetExperienceLevel(age);
        var experienceMultiplier = _experienceMultipliers[experienceLevel];

        // Complex bonus calculation with market data integration
        var marketAdjustment = GetMarketAdjustment(department);
        var performanceBonus = CalculatePerformanceBonus(baseSalary, departmentMultiplier);

        // Stock options based on complex vesting schedule
        var stockOptions = CalculateStockOptions(baseSalary, experienceLevel, department);

        // Risk assessment for total compensation
        var riskFactor = AssessRiskFactor(age, department, email);
        var adjustedBonus = performanceBonus * riskFactor;

        return new SalaryAnalysis
        {
            BaseSalary = baseSalary,
            AnnualBonus = adjustedBonus + marketAdjustment,
            StockOptions = stockOptions,
            TotalCompensation = baseSalary + adjustedBonus + marketAdjustment + (stockOptions * 50), // Assuming $50 per option
            DepartmentMultiplier = departmentMultiplier,
            ExperienceMultiplier = experienceMultiplier,
            RiskFactor = riskFactor
        };
    }

    private int GetExperienceLevel(int age) => age switch
    {
        >= 18 and <= 25 => 1,
        >= 26 and <= 35 => 2,
        >= 36 and <= 45 => 3,
        >= 46 and <= 55 => 4,
        _ => 5
    };

    private decimal GetMarketAdjustment(string department)
    {
        // Simulating complex market data analysis
        // In reality, this would integrate with external APIs, databases, etc.
        return department switch
        {
            "Engineering" => 5000m,
            "Sales" => 3000m,
            "Marketing" => 2000m,
            _ => 1000m
        };
    }

    private decimal CalculatePerformanceBonus(decimal baseSalary, decimal departmentMultiplier)
    {
        // Complex performance calculation
        return baseSalary * 0.15m * departmentMultiplier;
    }

    private int CalculateStockOptions(decimal baseSalary, int experienceLevel, string department)
    {
        var baseOptions = (int)(baseSalary / 1000);
        var experienceBonus = experienceLevel * 100;
        var departmentBonus = department == "Engineering" ? 200 : 50;

        return baseOptions + experienceBonus + departmentBonus;
    }

    private decimal AssessRiskFactor(int age, string department, string email)
    {
        // Complex risk assessment algorithm
        decimal riskScore = 1.0m;

        // Age-based risk
        if (age < 25) riskScore *= 0.9m;
        if (age > 55) riskScore *= 0.95m;

        // Department-based risk
        if (department == "Sales") riskScore *= 1.1m;

        // Email domain risk (simplified)
        if (!string.IsNullOrEmpty(email) && !email.EndsWith(".com")) riskScore *= 0.85m;

        return Math.Max(0.7m, Math.Min(1.3m, riskScore));
    }
}

public class SalaryAnalysis
{
    public decimal BaseSalary { get; set; }
    public decimal AnnualBonus { get; set; }
    public int StockOptions { get; set; }
    public decimal TotalCompensation { get; set; }
    public decimal DepartmentMultiplier { get; set; }
    public decimal ExperienceMultiplier { get; set; }
    public decimal RiskFactor { get; set; }
}

