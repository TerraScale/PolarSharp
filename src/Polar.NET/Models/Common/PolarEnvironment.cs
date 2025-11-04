namespace Polar.NET.Models.Common;

/// <summary>
/// Represents the Polar API environment.
/// </summary>
public enum PolarEnvironment
{
    /// <summary>
    /// Production environment for real customers and live payments.
    /// </summary>
    Production,

    /// <summary>
    /// Sandbox environment for safe testing and integration work.
    /// </summary>
    Sandbox
}