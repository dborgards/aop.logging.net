namespace AOP.Logging.Core.Attributes;

/// <summary>
/// Marks a parameter for logging. Can be used to control parameter logging behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class LogParameterAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to skip logging this parameter.
    /// </summary>
    public bool Skip { get; set; } = false;

    /// <summary>
    /// Gets or sets a custom name for the parameter in logs.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the maximum length for string values. Values longer than this will be truncated.
    /// </summary>
    public int MaxLength { get; set; } = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogParameterAttribute"/> class.
    /// </summary>
    public LogParameterAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogParameterAttribute"/> class with a custom name.
    /// </summary>
    /// <param name="name">The custom name to use in logs.</param>
    public LogParameterAttribute(string name)
    {
        Name = name;
    }
}
