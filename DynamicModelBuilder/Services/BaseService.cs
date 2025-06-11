using System.Reflection;

namespace DynamicModelBuilder.Services;

public abstract class BaseService
{
    protected T GetRequiredProperty<T>(object obj, string propertyName)
    {
        var value = GetPropertyValue<T>(obj, propertyName);
        if (value == null || value.Equals(default(T)))
        {
            throw new InvalidOperationException($"Required property '{propertyName}' is missing or has default value in type '{obj.GetType().Name}'");
        }
        return value;
    }

    protected T GetOptionalProperty<T>(object obj, string propertyName, T defaultValue = default(T))
    {
        var value = GetPropertyValue<T>(obj, propertyName);
        return value != null && !value.Equals(default(T)) ? value : defaultValue;
    }

    private T GetPropertyValue<T>(object obj, string propertyName)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj), "Object cannot be null when accessing properties");
        }

        try
        {
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanRead)
            {
                var value = property.GetValue(obj);
                if (value is T typedValue)
                    return typedValue;

                // Try to convert if possible
                if (value != null && typeof(T) != typeof(string))
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch (InvalidCastException)
                    {
                        // If conversion fails, return default
                        return default(T);
                    }
                }

                // Handle string conversion specially
                if (typeof(T) == typeof(string) && value != null)
                {
                    return (T)(object)value.ToString();
                }
            }
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            // Log the error but don't throw - return default instead
            Console.WriteLine($"Warning: Error accessing property '{propertyName}' on type '{obj.GetType().Name}': {ex.Message}");
        }

        return default(T);
    }

    protected void ValidateRequiredProperties(object obj, params string[] requiredPropertyNames)
    {
        var missingProperties = new List<string>();

        foreach (var propertyName in requiredPropertyNames)
        {
            try
            {
                var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanRead)
                {
                    missingProperties.Add(propertyName);
                }
            }
            catch
            {
                missingProperties.Add(propertyName);
            }
        }

        if (missingProperties.Any())
        {
            throw new InvalidOperationException($"Missing required properties in type '{obj.GetType().Name}': {string.Join(", ", missingProperties)}");
        }
    }
}