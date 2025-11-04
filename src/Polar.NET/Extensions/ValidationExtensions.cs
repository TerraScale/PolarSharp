using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Extensions;

/// <summary>
/// Extension methods for validating objects using DataAnnotations.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates an object using DataAnnotations and throws a ValidationException if invalid.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated (optional).</param>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public static void ValidateAndThrow(this object obj, string? parameterName = null)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(obj);
        
        if (!Validator.TryValidateObject(obj, validationContext, validationResults, true))
        {
            var errorMessages = validationResults
                .Select(r => r.ErrorMessage)
                .Where(msg => !string.IsNullOrEmpty(msg))
                .ToList();
            
            if (errorMessages.Any())
            {
                var errorMessage = string.Join("; ", errorMessages);
                throw new ValidationException(errorMessage);
            }
        }
    }

    /// <summary>
    /// Validates an object using DataAnnotations and returns validation results.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <returns>A list of validation results.</returns>
    public static List<ValidationResult> Validate(this object obj)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(obj);
        
        Validator.TryValidateObject(obj, validationContext, validationResults, true);
        
        return validationResults;
    }

    /// <summary>
    /// Validates an object using DataAnnotations and returns whether it is valid.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <returns>true if the object is valid; otherwise, false.</returns>
    public static bool IsValid(this object obj)
    {
        return obj.Validate().Count == 0;
    }
}