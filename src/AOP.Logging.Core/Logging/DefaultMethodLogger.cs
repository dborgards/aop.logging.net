using AOP.Logging.Core.Configuration;
using AOP.Logging.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace AOP.Logging.Core.Logging;

/// <summary>
/// Default implementation of <see cref="IMethodLogger"/>.
/// </summary>
public class DefaultMethodLogger : IMethodLogger
{
    private readonly ILogger<DefaultMethodLogger> _logger;
    private readonly AopLoggingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultMethodLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The AOP logging options.</param>
    public DefaultMethodLogger(ILogger<DefaultMethodLogger> logger, IOptions<AopLoggingOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public void LogEntry(string className, string methodName, IDictionary<string, object?> parameters, LogLevel logLevel)
    {
        if (!_logger.IsEnabled(logLevel))
        {
            return;
        }

        if (_options.UseStructuredLogging)
        {
            var state = new Dictionary<string, object?>
            {
                ["ClassName"] = className,
                ["MethodName"] = methodName
            };

            // Only include Parameters in state if referenced in the message template
            // This prevents unintended data leakage to log sinks that capture all state properties
            var includeParametersInState = _options.EntryMessageFormat.Contains("{Parameters}");
            if (includeParametersInState)
            {
                state["Parameters"] = FormatParameters(parameters);
            }

            // Only include individual parameters in state if they're referenced in the template
            foreach (var param in parameters)
            {
                var paramPlaceholder = $"{{Param_{param.Key}}}";
                if (_options.EntryMessageFormat.Contains(paramPlaceholder))
                {
                    state[$"Param_{param.Key}"] = FormatValue(param.Value);
                }
            }

            _logger.Log(logLevel, 0, state, null, (s, _) =>
            {
                var message = _options.EntryMessageFormat
                    .Replace("{ClassName}", className)
                    .Replace("{MethodName}", methodName);

                // Only format parameters if the template uses them
                if (_options.EntryMessageFormat.Contains("{Parameters}"))
                {
                    message = message.Replace("{Parameters}", FormatParameters(parameters));
                }

                return message;
            });
        }
        else
        {
            var message = _options.EntryMessageFormat
                .Replace("{ClassName}", className)
                .Replace("{MethodName}", methodName);

            // Only format parameters if the template uses them
            if (_options.EntryMessageFormat.Contains("{Parameters}"))
            {
                message = message.Replace("{Parameters}", FormatParameters(parameters));
            }

            _logger.Log(logLevel, message);
        }
    }

    /// <inheritdoc/>
    public void LogExit(string className, string methodName, object? returnValue, long executionTimeMs, LogLevel logLevel)
    {
        if (!_logger.IsEnabled(logLevel))
        {
            return;
        }

        if (_options.UseStructuredLogging)
        {
            var state = new Dictionary<string, object?>
            {
                ["ClassName"] = className,
                ["MethodName"] = methodName
            };

            // Only include ReturnValue in state if referenced in the message template
            // This prevents unintended data leakage to log sinks that capture all state properties
            if (_options.ExitMessageFormat.Contains("{ReturnValue}"))
            {
                state["ReturnValue"] = FormatValue(returnValue);
            }

            // Only include ExecutionTime in state if referenced in the message template
            if (_options.ExitMessageFormat.Contains("{ExecutionTime}"))
            {
                state["ExecutionTime"] = executionTimeMs;
            }

            _logger.Log(logLevel, 0, state, null, (s, _) =>
            {
                // Check template once to avoid multiple scans
                var template = _options.ExitMessageFormat;
                var hasReturnValue = template.Contains("{ReturnValue}");
                var hasExecutionTime = template.Contains("{ExecutionTime}");

                var message = template
                    .Replace("{ClassName}", className)
                    .Replace("{MethodName}", methodName);

                // Only format return value if the template uses it
                if (hasReturnValue)
                {
                    message = message.Replace("{ReturnValue}", FormatValue(returnValue)?.ToString() ?? "null");
                }

                // Only format execution time if the template uses it
                if (hasExecutionTime)
                {
                    message = message.Replace("{ExecutionTime}", executionTimeMs.ToString());
                }

                return message;
            });
        }
        else
        {
            // Check template once to avoid multiple scans
            var template = _options.ExitMessageFormat;
            var hasReturnValue = template.Contains("{ReturnValue}");
            var hasExecutionTime = template.Contains("{ExecutionTime}");

            var message = template
                .Replace("{ClassName}", className)
                .Replace("{MethodName}", methodName);

            // Only format return value if the template uses it
            if (hasReturnValue)
            {
                message = message.Replace("{ReturnValue}", FormatValue(returnValue)?.ToString() ?? "null");
            }

            // Only format execution time if the template uses it
            if (hasExecutionTime)
            {
                message = message.Replace("{ExecutionTime}", executionTimeMs.ToString());
            }

            _logger.Log(logLevel, message);
        }
    }

    /// <inheritdoc/>
    public void LogException(string className, string methodName, Exception exception, long executionTimeMs, LogLevel logLevel)
    {
        if (!_logger.IsEnabled(logLevel))
        {
            return;
        }

        if (_options.UseStructuredLogging)
        {
            var state = new Dictionary<string, object?>
            {
                ["ClassName"] = className,
                ["MethodName"] = methodName
            };

            // Only include exception details in state if referenced in the message template
            // This prevents unintended data leakage to log sinks that capture all state properties
            if (_options.ExceptionMessageFormat.Contains("{ExceptionType}"))
            {
                state["ExceptionType"] = exception.GetType().Name;
            }

            if (_options.ExceptionMessageFormat.Contains("{ExceptionMessage}"))
            {
                state["ExceptionMessage"] = exception.Message;
            }

            if (_options.ExceptionMessageFormat.Contains("{ExecutionTime}"))
            {
                state["ExecutionTime"] = executionTimeMs;
            }

            _logger.Log(logLevel, 0, state, exception, (s, _) =>
                _options.ExceptionMessageFormat
                    .Replace("{ClassName}", className)
                    .Replace("{MethodName}", methodName)
                    .Replace("{ExceptionType}", exception.GetType().Name)
                    .Replace("{ExceptionMessage}", exception.Message)
                    .Replace("{ExecutionTime}", executionTimeMs.ToString()));
        }
        else
        {
            var message = _options.ExceptionMessageFormat
                .Replace("{ClassName}", className)
                .Replace("{MethodName}", methodName)
                .Replace("{ExceptionType}", exception.GetType().Name)
                .Replace("{ExceptionMessage}", exception.Message)
                .Replace("{ExecutionTime}", executionTimeMs.ToString());

            _logger.Log(logLevel, exception, message);
        }
    }

    private string FormatParameters(IDictionary<string, object?> parameters)
    {
        if (parameters.Count == 0)
        {
            return "no parameters";
        }

        // Use StringBuilder to reduce string allocations
        var sb = new StringBuilder();
        var first = true;

        foreach (var param in parameters)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            first = false;

            sb.Append(param.Key);
            sb.Append('=');
            sb.Append(FormatValue(param.Value));
        }

        return sb.ToString();
    }

    private object? FormatValue(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        var valueType = value.GetType();

        // Handle strings
        if (value is string stringValue)
        {
            if (stringValue.Length > _options.MaxStringLength)
            {
                return $"\"{stringValue.Substring(0, _options.MaxStringLength)}...\" (truncated from {stringValue.Length})";
            }
            return $"\"{stringValue}\"";
        }

        // Handle collections - use deferred enumeration to prevent DoS from infinite/large enumerables
        if (value is System.Collections.IEnumerable enumerable and not string)
        {
            var sb = new StringBuilder();
            var count = 0;
            var truncated = false;

            sb.Append('[');

            foreach (var item in enumerable)
            {
                if (count >= _options.MaxCollectionSize)
                {
                    truncated = true;
                    break;
                }

                if (count > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(FormatValue(item));
                count++;
            }

            sb.Append(']');

            if (truncated)
            {
                // Wrap with truncation message using StringBuilder to avoid allocation
                sb.Insert(0, $"[Collection with {_options.MaxCollectionSize}+ items (showing first {_options.MaxCollectionSize}): ");
                sb.Append(']');
            }

            return sb.ToString();
        }

        // Handle primitives and other types
        return value;
    }
}
