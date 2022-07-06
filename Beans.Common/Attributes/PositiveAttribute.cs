using System.ComponentModel.DataAnnotations;

namespace Beans.Common.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class PositiveAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new("Value is null");
        }
        if (validationContext is null)
        {
            return new("Validation context is null");
        }
        var valid = value switch
        {
            float fval => fval > 0.0f,
            double dval => dval > 0.0,
            int ival => ival > 0,
            long lval => lval > 0,
            decimal mval => mval > 0M,
            _ => false
        };
        if (valid)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(ErrorMessage))
        {
            var prop = validationContext.DisplayName;
            return new($"{prop} must be a number greater than zero");
        }
        return new(ErrorMessage);
    }
}
