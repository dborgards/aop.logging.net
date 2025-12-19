using Microsoft.Extensions.Logging;

namespace AOP.Logging.Core.Attributes;

/// <summary>
/// Controls exception logging behavior for a method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class LogExceptionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the log level for exceptions.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Error;

    /// <summary>
    /// Gets or sets whether to include the full exception details (stack trace, inner exceptions).
    /// </summary>
    public bool IncludeDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to rethrow the exception after logging.
    /// </summary>
    public bool Rethrow { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom message template for exception logging.
    /// Use {MethodName}, {ClassName}, {ExceptionType}, and {ExceptionMessage} as placeholders.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogExceptionAttribute"/> class.
    /// </summary>
    public LogExceptionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogExceptionAttribute"/> class with a specific log level.
    /// </summary>
    /// <param name="logLevel">The log level to use for exceptions.</param>
    public LogExceptionAttribute(LogLevel logLevel)
    {
        LogLevel = logLevel;
    }
}
