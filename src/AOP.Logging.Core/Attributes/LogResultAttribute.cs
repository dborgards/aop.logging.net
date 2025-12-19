namespace AOP.Logging.Core.Attributes;

/// <summary>
/// Explicitly marks that a method's return value should be logged.
/// Can be used to override class or method-level settings.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class LogResultAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to skip logging the return value.
    /// </summary>
    public bool Skip { get; set; } = false;

    /// <summary>
    /// Gets or sets a custom name for the return value in logs.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the maximum length for string return values. Values longer than this will be truncated.
    /// </summary>
    public int MaxLength { get; set; } = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogResultAttribute"/> class.
    /// </summary>
    public LogResultAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogResultAttribute"/> class with a custom name.
    /// </summary>
    /// <param name="name">The custom name to use in logs.</param>
    public LogResultAttribute(string name)
    {
        Name = name;
    }
}
