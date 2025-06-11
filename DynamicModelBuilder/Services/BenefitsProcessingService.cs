using DynamicModelBuilder.Models;

namespace DynamicModelBuilder.Services;

public class BenefitsProcessingService : BaseService
{
    public BenefitsPackage ProcessBenefitsEligibility(object person)
    {
        // Use reflection to safely access properties with fallbacks
        var age = GetRequiredProperty<int>(person, "Age");
        var salary = GetOptionalProperty<decimal>(person, "Salary", 40000m);
        var department = GetOptionalProperty<string>(person, "Department", "Unknown");

        // Complex benefits calculation with multiple external integrations
        var healthPremium = CalculateHealthInsurancePremium(age, department);
        var retirementMatch = Calculate401kMatch(salary, age);
        var vacationDays = CalculateVacationEntitlement(age, department);
        var lifeInsurance = CalculateLifeInsurance(salary);

        return new BenefitsPackage
        {
            HealthInsurancePremium = healthPremium,
            RetirementMatch = retirementMatch,
            VacationDays = vacationDays,
            LifeInsuranceCoverage = lifeInsurance,
            IsEligibleForDentalPlan = age >= 21,
            IsEligibleForVisionPlan = true,
            FlexSpendingAccount = salary > 50000 ? 2500m : 1500m
        };
    }

    private decimal CalculateHealthInsurancePremium(int age, string department)
    {
        // Complex actuarial calculations
        decimal basePremium = 400m;

        // Age-based adjustments
        if (age > 40) basePremium *= 1.2m;
        if (age > 50) basePremium *= 1.4m;

        // Department-based risk adjustments
        if (department == "Engineering") basePremium *= 0.95m; // Lower risk

        return basePremium;
    }

    private decimal Calculate401kMatch(decimal salary, int age)
    {
        // Company matches up to 6% with complex vesting schedule
        decimal matchPercentage = 0.06m;
        decimal maxMatch = salary * matchPercentage;

        // Vesting based on age (proxy for tenure)
        decimal vestingPercentage = age switch
        {
            < 25 => 0.25m,
            < 30 => 0.50m,
            < 40 => 0.75m,
            _ => 1.0m
        };

        return maxMatch * vestingPercentage;
    }

    private int CalculateVacationEntitlement(int age, string department)
    {
        // Complex vacation calculation
        int baseDays = 15;

        // Age-based experience proxy
        if (age >= 30) baseDays += 5;
        if (age >= 40) baseDays += 5;
        if (age >= 50) baseDays += 5;

        // Department bonuses
        if (department == "Sales") baseDays += 3;

        return Math.Min(baseDays, 30); // Cap at 30 days
    }

    private decimal CalculateLifeInsurance(decimal salary)
    {
        // Life insurance typically 2x salary with caps
        return Math.Min(salary * 2, 500000m);
    }
}

public class BenefitsPackage
{
    public decimal HealthInsurancePremium { get; set; }
    public decimal RetirementMatch { get; set; }
    public int VacationDays { get; set; }
    public decimal LifeInsuranceCoverage { get; set; }
    public bool IsEligibleForDentalPlan { get; set; }
    public bool IsEligibleForVisionPlan { get; set; }
    public decimal FlexSpendingAccount { get; set; }
}