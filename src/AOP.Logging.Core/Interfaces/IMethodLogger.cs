using Microsoft.Extensions.Logging;

namespace AOP.Logging.Core.Interfaces;

/// <summary>
/// Defines the contract for method logging operations.
/// </summary>
public interface IMethodLogger
{
    /// <summary>
    /// Logs method entry with parameters.
    /// </summary>
    /// <param name="className">The name of the class containing the method.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameters">The method parameters as key-value pairs.</param>
    /// <param name="logLevel">The log level to use.</param>
    void LogEntry(string className, string methodName, IDictionary<string, object?> parameters, LogLevel logLevel);

    /// <summary>
    /// Logs method exit with return value and execution time.
    /// </summary>
    /// <param name="className">The name of the class containing the method.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnValue">The return value of the method.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="logLevel">The log level to use.</param>
    void LogExit(string className, string methodName, object? returnValue, long executionTimeMs, LogLevel logLevel);

    /// <summary>
    /// Logs an exception that occurred during method execution.
    /// </summary>
    /// <param name="className">The name of the class containing the method.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds before the exception.</param>
    /// <param name="logLevel">The log level to use.</param>
    void LogException(string className, string methodName, Exception exception, long executionTimeMs, LogLevel logLevel);
}
