using AOP.Logging.Core.Configuration;
using AOP.Logging.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
                _options.EntryMessageFormat
                    .Replace("{ClassName}", className)
                    .Replace("{MethodName}", methodName)
                    .Replace("{Parameters}", FormatParameters(parameters)));
        }
        else
        {
            var message = _options.EntryMessageFormat
                .Replace("{ClassName}", className)
                .Replace("{MethodName}", methodName)
                .Replace("{Parameters}", FormatParameters(parameters));

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
                _options.ExitMessageFormat
                    .Replace("{ClassName}", className)
                    .Replace("{MethodName}", methodName)
                    .Replace("{ReturnValue}", FormatValue(returnValue)?.ToString() ?? "null")
                    .Replace("{ExecutionTime}", executionTimeMs.ToString()));
        }
        else
        {
            var message = _options.ExitMessageFormat
                .Replace("{ClassName}", className)
                .Replace("{MethodName}", methodName)
                .Replace("{ReturnValue}", FormatValue(returnValue)?.ToString() ?? "null")
                .Replace("{ExecutionTime}", executionTimeMs.ToString());

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

        var formattedParams = parameters.Select(p => $"{p.Key}={FormatValue(p.Value)}");
        return string.Join(", ", formattedParams);
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
            // Use Take to limit enumeration without materializing the entire collection
            var limitedItems = enumerable.Cast<object>().Take(_options.MaxCollectionSize + 1).ToList();

            if (limitedItems.Count > _options.MaxCollectionSize)
            {
                // Collection is larger than the limit, show truncated message
                // Use GetRange to avoid redundant enumeration since limitedItems is already materialized
                var truncatedItems = limitedItems.GetRange(0, _options.MaxCollectionSize);
                return $"[Collection with {_options.MaxCollectionSize}+ items (showing first {_options.MaxCollectionSize}): {string.Join(", ", truncatedItems.Select(FormatValue))}]";
            }

            return $"[{string.Join(", ", limitedItems.Select(FormatValue))}]";
        }

        // Handle primitives and other types
        return value;
    }
}
