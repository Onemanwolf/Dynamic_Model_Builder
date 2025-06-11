using System.Text.Json;
using System.Text.RegularExpressions;

namespace DynamicModelBuilder.Models
{
    public class ModelBuilder
    {
        public class PropertyDefinition
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public List<RuleDefinition> Rules { get; set; }
            public List<string> Specifications { get; set; } = new();
        }

        public class RuleDefinition
        {
            public string RuleType { get; set; }
            public object Value { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class SpecificationDefinition
        {
            public string Name { get; set; }
            public string Expression { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class DomainRuleDefinition
        {
            public string Name { get; set; }
            public string Expression { get; set; }
            public string ReturnType { get; set; }
        }

        public class ModelDefinition
        {
            public string OutputPath { get; set; }
            public string ClassName { get; set; }
            public List<PropertyDefinition> Properties { get; set; } = new();
            public List<SpecificationDefinition> Specifications { get; set; } = new();
            public List<DomainRuleDefinition> DomainRules { get; set; } = new();
        }

        public class DynamicModel
        {
            private readonly Dictionary<string, object> _values = new();
            private readonly Dictionary<string, List<RuleDefinition>> _rules;
            private readonly Dictionary<string, SpecificationDefinition> _specifications;
            private readonly List<DomainRuleDefinition> _domainRules;

            public DynamicModel(Dictionary<string, List<RuleDefinition>> rules,
                               Dictionary<string, SpecificationDefinition> specifications,
                               List<DomainRuleDefinition> domainRules)
            {
                _rules = rules;
                _specifications = specifications;
                _domainRules = domainRules;
            }

            public void SetProperty(string name, object value)
            {
                if (_rules.ContainsKey(name))
                {
                    var validationErrors = ValidateProperty(name, value);
                    if (validationErrors.Any())
                    {
                        throw new ArgumentException($"Validation failed for {name}: {string.Join(", ", validationErrors)}");
                    }
                }
                _values[name] = value;
            }

            public object GetProperty(string name)
            {
                return _values.ContainsKey(name) ? _values[name] : null;
            }

            private List<string> ValidateProperty(string name, object value)
            {
                var errors = new List<string>();
                var rules = _rules[name];

                foreach (var rule in rules)
                {
                    switch (rule.RuleType.ToLower())
                    {
                        case "required":
                            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                            {
                                errors.Add(rule.ErrorMessage ?? $"{name} is required");
                            }
                            break;

                        case "minlength":
                            if (value is string strValue && rule.Value is int minLength && strValue.Length < minLength)
                            {
                                errors.Add(rule.ErrorMessage ?? $"{name} must be at least {minLength} characters");
                            }
                            break;

                        case "maxlength":
                            if (value is string strValueMax && rule.Value is int maxLength && strValueMax.Length > maxLength)
                            {
                                errors.Add(rule.ErrorMessage ?? $"{name} must not exceed {maxLength} characters");
                            }
                            break;

                        case "range":
                            if (value is IComparable comparable && rule.Value is JsonElement range)
                            {
                                var min = range.GetProperty("min").GetDouble();
                                var max = range.GetProperty("max").GetDouble();
                                var val = Convert.ToDouble(comparable);
                                if (val < min || val > max)
                                {
                                    errors.Add(rule.ErrorMessage ?? $"{name} must be between {min} and {max}");
                                }
                            }
                            break;
                    }
                }

                return errors;
            }

            public bool EvaluateSpecification(string specificationName)
            {
                if (!_specifications.ContainsKey(specificationName))
                    return true;

                var spec = _specifications[specificationName];
                return EvaluateExpression(spec.Expression);
            }

            private bool EvaluateExpression(string expression)
            {
                // Simple expression evaluator for demo purposes
                // In production, consider using a proper expression parser like NCalc

                // Replace property names with actual values
                var evaluatedExpression = expression;
                foreach (var kvp in _values)
                {
                    var propertyValue = kvp.Value?.ToString() ?? "";
                    evaluatedExpression = evaluatedExpression.Replace(kvp.Key, $"\"{propertyValue}\"");
                }

                // Handle basic operators (this is a simplified implementation)
                evaluatedExpression = evaluatedExpression
                    .Replace(" AND ", " && ")
                    .Replace(" OR ", " || ")
                    .Replace(" CONTAINS ", ".Contains")
                    .Replace(" ENDS_WITH ", ".EndsWith");

                // For demo purposes, return true (in production, use proper expression evaluation)
                return true;
            }

            public void SaveAsClass(string className, string outputPath)
            {
                // Find the project root by traversing up until a .csproj file is found
                var dir = Directory.GetCurrentDirectory();
                string projectRoot = null;
                while (dir != null)
                {
                    var csproj = Directory.GetFiles(dir, "*.csproj").FirstOrDefault();
                    if (csproj != null)
                    {
                        projectRoot = dir;
                        break;
                    }
                    dir = Directory.GetParent(dir)?.FullName;
                }

                // Fallback: if not found, use current directory
                if (projectRoot == null)
                    projectRoot = Directory.GetCurrentDirectory();

                // Combine the project root with the output path from JSON
                var modelsDir = Path.Combine(projectRoot, outputPath);
                Directory.CreateDirectory(modelsDir);

                // Generate DTO properties with validation attributes
                var dtoProperties = _rules.Keys.Select(name =>
                {
                    var rules = _rules[name];
                    var attributes = new List<string>();
                    string type = GetPropertyType(name);

                    foreach (var rule in rules)
                    {
                        switch (rule.RuleType.ToLower())
                        {
                            case "required":
                                if (!string.IsNullOrEmpty(rule.ErrorMessage))
                                    attributes.Add($"[Required(ErrorMessage = \"{rule.ErrorMessage}\")]");
                                else
                                    attributes.Add("[Required]");
                                break;
                            case "minlength":
                                if (rule.Value is int minLen)
                                    attributes.Add($"[MinLength({minLen})]");
                                break;
                            case "maxlength":
                                if (rule.Value is int maxLen)
                                    attributes.Add($"[MaxLength({maxLen})]");
                                break;
                            case "range":
                                if (rule.Value is JsonElement range)
                                {
                                    var min = range.GetProperty("min").GetDouble();
                                    var max = range.GetProperty("max").GetDouble();
                                    attributes.Add($"[Range({min}, {max})]");
                                }
                                break;
                            case "email":
                                attributes.Add("[EmailAddress]");
                                break;
                        }
                    }

                    var attributeString = attributes.Count > 0 ? string.Join(Environment.NewLine + "        ", attributes) + Environment.NewLine + "        " : "";
                    return $"{attributeString}public {type} {name} {{ get; set; }}";
                });

                // Generate Domain properties and methods
                var domainProperties = _rules.Keys.Select(name =>
                {
                    string type = GetPropertyType(name);
                    return $"public {type} {name} {{ get; set; }}";
                });

                // Generate specification methods
                var specificationMethods = _specifications.Values.Select(spec =>
                {
                    var methodBody = GenerateSpecificationMethodBody(spec);
                    return $@"
        /// <summary>
        /// {spec.ErrorMessage ?? $"Validates {spec.Name} specification"}
        /// </summary>
        public bool {spec.Name}()
        {{
            {methodBody}
        }}";
                });

                // Generate domain rule methods
                var domainRuleMethods = _domainRules.Select(rule =>
                {
                    var methodBody = GenerateDomainRuleMethodBody(rule);
                    return $@"
        /// <summary>
        /// Domain rule: {rule.Name}
        /// </summary>
        public {rule.ReturnType} {rule.Name}()
        {{
            {methodBody}
        }}";
                });

                // Generate DTO class content
                var dtoClassContent = $@"
using System;
using System.ComponentModel.DataAnnotations;

namespace DynamicModelBuilder.Models
{{
    public class {className}Dto
    {{
        {string.Join(Environment.NewLine + Environment.NewLine + "        ", dtoProperties)}
    }}
}}";

                // Generate Domain class content with specifications and domain logic
                var domainClassContent = $@"
using System;

namespace DynamicModelBuilder.Models
{{
    public class {className}
    {{
        {string.Join(Environment.NewLine + "        ", domainProperties)}
{string.Join("", specificationMethods)}
{string.Join("", domainRuleMethods)}
    }}
}}";

                // Save both files
                File.WriteAllText(Path.Combine(modelsDir, $"{className}Dto.cs"), dtoClassContent);
                File.WriteAllText(Path.Combine(modelsDir, $"{className}.cs"), domainClassContent);
            }

            private string GetPropertyType(string propertyName)
            {
                var rule = _rules[propertyName].FirstOrDefault();
                if (rule == null) return "object";

                return rule.RuleType.ToLower() switch
                {
                    "range" => "int",
                    "minlength" or "maxlength" or "required" or "email" => "string",
                    _ => "object"
                };
            }

            private string GenerateSpecificationMethodBody(SpecificationDefinition spec)
            {
                var expression = CleanExpression(spec.Expression);
                return $"return {expression};";
            }

            private string GenerateDomainRuleMethodBody(DomainRuleDefinition rule)
            {
                var expression = CleanExpression(rule.Expression);

                // Handle string concatenation for string return types
                if (rule.ReturnType == "string")
                {
                    expression = FixStringConcatenation(expression);
                }

                return $"return {expression};";
            }

            private string CleanExpression(string expression)
{
    // Step 1: Replace single quotes with double quotes for string literals FIRST
    expression = Regex.Replace(expression, @"'([^']*)'", "\"$1\"");

    // Step 2: Replace operators
    expression = expression
        .Replace(" AND ", " && ")
        .Replace(" OR ", " || ")
        .Replace(" CONTAINS ", ".Contains")
        .Replace(" ENDS_WITH ", ".EndsWith");

    // Step 3: Fix spacing around comparison operators
    expression = Regex.Replace(expression, @"\s*(>=|<=|==|!=|>|<)\s*", " $1 ");

    // Step 4: Remove extra whitespace
    expression = Regex.Replace(expression, @"\s+", " ").Trim();

    return expression;
}

            private string FixStringConcatenation(string expression)
            {
                // Replace single quotes with double quotes for string literals
                expression = Regex.Replace(expression, @"'([^']*)'", "\"$1\"");

                // Fix spacing around + operator for string concatenation
                expression = Regex.Replace(expression, @"\s*\+\s*", " + ");

                return expression;
            }
        }

        public static DynamicModel CreateModel(string jsonDefinition)
        {
            var modelDef = JsonSerializer.Deserialize<ModelDefinition>(jsonDefinition);

            var rules = modelDef.Properties.ToDictionary(
                p => p.Name,
                p => p.Rules ?? new List<RuleDefinition>()
            );

            var specifications = modelDef.Specifications.ToDictionary(
                s => s.Name,
                s => s
            );

            return new DynamicModel(rules, specifications, modelDef.DomainRules);
        }
    }
}