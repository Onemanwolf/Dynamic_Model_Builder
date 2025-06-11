using DynamicModelBuilder.Models;

namespace DynamicModelBuilder.Services;

public class ComplianceValidationService : BaseService
{
    private readonly List<string> _highSecurityDepartments = new() { "Engineering", "Finance" };
    private readonly Dictionary<string, int> _departmentRiskScores = new()
    {
        ["Engineering"] = 85,
        ["Finance"] = 90,
        ["Sales"] = 70,
        ["Marketing"] = 65,
        ["HR"] = 80
    };

    public ComplianceResult ValidateEmployeeCompliance(object person)
    {
        // Use BaseService methods for safe property access with fallbacks
        var department = GetOptionalProperty<string>(person, "Department", "Unknown");
        var salary = GetOptionalProperty<decimal>(person, "Salary", 40000m);
        var age = GetRequiredProperty<int>(person, "Age"); // Age is required for compliance
        var email = GetOptionalProperty<string>(person, "Email", "");

        var backgroundCheckRequired = RequiresBackgroundCheck(department, salary, age);
        var securityClearance = DetermineSecurityClearanceLevel(department, salary);
        var complianceScore = CalculateOverallComplianceScore(department, salary, age, email);
        var regulatoryFlags = CheckRegulatoryCompliance(department, salary, age, email);

        return new ComplianceResult
        {
            BackgroundCheckRequired = backgroundCheckRequired,
            SecurityClearanceLevel = securityClearance,
            ComplianceScore = complianceScore,
            RegulatoryFlags = regulatoryFlags,
            RequiresAdditionalScreening = complianceScore < 75,
            ClearanceExpirationDate = DateTime.Now.AddYears(securityClearance == "Top Secret" ? 5 : 10),
            ComplianceStatus = complianceScore >= 85 ? "Compliant" : complianceScore >= 70 ? "Conditional" : "Non-Compliant"
        };
    }

    private bool RequiresBackgroundCheck(string department, decimal salary, int age)
    {
        return _highSecurityDepartments.Contains(department) ||
               salary > 100000 ||
               age < 25;
    }

    private string DetermineSecurityClearanceLevel(string department, decimal salary)
    {
        if (department == "Finance" && salary > 80000)
            return "Top Secret";

        if (_highSecurityDepartments.Contains(department))
            return "Secret";

        if (salary > 75000)
            return "Confidential";

        return "Public";
    }

    private int CalculateOverallComplianceScore(string department, decimal salary, int age, string email)
    {
        int baseScore = _departmentRiskScores.GetValueOrDefault(department, 60);

        // Age-based adjustments
        if (age >= 30) baseScore += 5;
        if (age >= 40) baseScore += 5;
        if (age < 25) baseScore -= 10;

        // Salary-based adjustments
        if (salary > 100000) baseScore += 10;
        if (salary < 40000) baseScore -= 5;

        // Email domain validation
        if (email.Contains("@company.com")) baseScore += 5;
        else if (!string.IsNullOrEmpty(email)) baseScore -= 10;

        return Math.Max(0, Math.Min(100, baseScore));
    }

    private List<string> CheckRegulatoryCompliance(string department, decimal salary, int age, string email)
    {
        var flags = new List<string>();

        // GDPR compliance checks
        if (!email.Contains("@") && !string.IsNullOrEmpty(email))
            flags.Add("GDPR: Invalid email format");

        // SOX compliance for finance
        if (department == "Finance" && salary > 75000)
            flags.Add("SOX: Enhanced financial controls required");

        // HIPAA compliance simulation
        if (department == "HR")
            flags.Add("HIPAA: PII handling training required");

        // Industry-specific compliance
        if (age < 21 && department == "Sales")
            flags.Add("Industry: Age restriction for client interaction");

        return flags;
    }
}

public class ComplianceResult
{
    public bool BackgroundCheckRequired { get; set; }
    public string SecurityClearanceLevel { get; set; }
    public int ComplianceScore { get; set; }
    public List<string> RegulatoryFlags { get; set; } = new();
    public bool RequiresAdditionalScreening { get; set; }
    public DateTime ClearanceExpirationDate { get; set; }
    public string ComplianceStatus { get; set; }
}