using Microsoft.Extensions.Logging;

namespace AOP.Logging.Core.Attributes;

/// <summary>
/// Marks a method for automatic logging.
/// Can be used to override class-level settings or enable logging for specific methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class LogMethodAttribute : Attribute
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
    /// Gets or sets whether to skip logging for this method (useful to exclude specific methods in a logged class).
    /// </summary>
    public bool Skip { get; set; } = false;

    /// <summary>
    /// Gets or sets a custom message template for method entry.
    /// Use {MethodName}, {ClassName}, and parameter names as placeholders.
    /// </summary>
    public string? EntryMessage { get; set; }

    /// <summary>
    /// Gets or sets a custom message template for method exit.
    /// Use {MethodName}, {ClassName}, {ExecutionTime}, and {ReturnValue} as placeholders.
    /// </summary>
    public string? ExitMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogMethodAttribute"/> class.
    /// </summary>
    public LogMethodAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogMethodAttribute"/> class with a specific log level.
    /// </summary>
    /// <param name="logLevel">The log level to use.</param>
    public LogMethodAttribute(LogLevel logLevel)
    {
        LogLevel = logLevel;
    }
}
