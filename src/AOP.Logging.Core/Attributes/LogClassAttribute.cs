using Microsoft.Extensions.Logging;

namespace AOP.Logging.Core.Attributes;

/// <summary>
/// Marks a class for automatic logging of all public methods.
/// Methods can be individually controlled with <see cref="LogMethodAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class LogClassAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the log level for method entry/exit logging.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets whether to log method execution time.
    /// </summary>
    public bool LogExecutionTime { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log method parameters.
    /// </summary>
    public bool LogParameters { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log method return values.
    /// </summary>
    public bool LogReturnValue { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log exceptions.
    /// </summary>
    public bool LogExceptions { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogClassAttribute"/> class.
    /// </summary>
    public LogClassAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogClassAttribute"/> class with a specific log level.
    /// </summary>
    /// <param name="logLevel">The log level to use.</param>
    public LogClassAttribute(LogLevel logLevel)
    {
        LogLevel = logLevel;
    }
}
