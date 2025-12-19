using Microsoft.Extensions.Logging;

namespace AOP.Logging.Core.Configuration;

/// <summary>
/// Configuration options for AOP logging framework.
/// </summary>
public class AopLoggingOptions
{
    /// <summary>
    /// Gets or sets the default log level for method entry/exit.
    /// </summary>
    public LogLevel DefaultLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets whether to log execution time by default.
    /// </summary>
    public bool LogExecutionTime { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log method parameters by default.
    /// </summary>
    public bool LogParameters { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log return values by default.
    /// </summary>
    public bool LogReturnValues { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log exceptions by default.
    /// </summary>
    public bool LogExceptions { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of namespaces to include for logging.
    /// If empty, all namespaces are included.
    /// </summary>
    public List<string> IncludedNamespaces { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of namespaces to exclude from logging.
    /// Excluded namespaces take precedence over included namespaces.
    /// </summary>
    public List<string> ExcludedNamespaces { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of class name patterns to include for logging.
    /// Supports wildcards (*).
    /// </summary>
    public List<string> IncludedClasses { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of class name patterns to exclude from logging.
    /// Supports wildcards (*). Excluded classes take precedence over included classes.
    /// </summary>
    public List<string> ExcludedClasses { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum string length for logged values.
    /// Values longer than this will be truncated.
    /// </summary>
    public int MaxStringLength { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum collection size to log.
    /// Collections larger than this will show only the count.
    /// </summary>
    public int MaxCollectionSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to use structured logging with separate parameters.
    /// </summary>
    public bool UseStructuredLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the format for entry log messages.
    /// Available placeholders: {ClassName}, {MethodName}, {Parameters}
    /// </summary>
    public string EntryMessageFormat { get; set; } = "Entering {ClassName}.{MethodName}";

    /// <summary>
    /// Gets or sets the format for exit log messages.
    /// Available placeholders: {ClassName}, {MethodName}, {ReturnValue}, {ExecutionTime}
    /// </summary>
    public string ExitMessageFormat { get; set; } = "Exiting {ClassName}.{MethodName} (took {ExecutionTime}ms)";

    /// <summary>
    /// Gets or sets the format for exception log messages.
    /// Available placeholders: {ClassName}, {MethodName}, {ExceptionType}, {ExceptionMessage}, {ExecutionTime}
    /// </summary>
    public string ExceptionMessageFormat { get; set; } = "Exception in {ClassName}.{MethodName}: {ExceptionType} - {ExceptionMessage}";

    /// <summary>
    /// Determines whether a namespace should be logged based on configuration.
    /// </summary>
    /// <param name="namespace">The namespace to check.</param>
    /// <returns>True if the namespace should be logged; otherwise, false.</returns>
    public bool ShouldLogNamespace(string @namespace)
    {
        // Check excluded namespaces first
        if (ExcludedNamespaces.Any(excluded => @namespace.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // If no included namespaces specified, include all
        if (IncludedNamespaces.Count == 0)
        {
            return true;
        }

        // Check included namespaces
        return IncludedNamespaces.Any(included => @namespace.StartsWith(included, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether a class should be logged based on configuration.
    /// </summary>
    /// <param name="className">The class name to check.</param>
    /// <returns>True if the class should be logged; otherwise, false.</returns>
    public bool ShouldLogClass(string className)
    {
        // Check excluded classes first
        if (ExcludedClasses.Any(excluded => MatchesPattern(className, excluded)))
        {
            return false;
        }

        // If no included classes specified, include all
        if (IncludedClasses.Count == 0)
        {
            return true;
        }

        // Check included classes
        return IncludedClasses.Any(included => MatchesPattern(className, included));
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (pattern == "*")
        {
            return true;
        }

        if (!pattern.Contains('*'))
        {
            return value.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }

        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(value, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
