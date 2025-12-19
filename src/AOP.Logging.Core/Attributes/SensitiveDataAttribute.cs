namespace AOP.Logging.Core.Attributes;

/// <summary>
/// Marks a parameter, property, or return value as containing sensitive data that should not be logged.
/// Sensitive data will be replaced with a masked value in logs.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.ReturnValue,
    AllowMultiple = false, Inherited = false)]
public sealed class SensitiveDataAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the mask value to use when logging sensitive data.
    /// </summary>
    public string MaskValue { get; set; } = "***SENSITIVE***";

    /// <summary>
    /// Gets or sets whether to show the length of the sensitive data (e.g., "***SENSITIVE(10)***").
    /// </summary>
    public bool ShowLength { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveDataAttribute"/> class.
    /// </summary>
    public SensitiveDataAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveDataAttribute"/> class with a custom mask value.
    /// </summary>
    /// <param name="maskValue">The mask value to use.</param>
    public SensitiveDataAttribute(string maskValue)
    {
        MaskValue = maskValue;
    }
}
